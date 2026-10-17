using System.Diagnostics;
using System.Text;

namespace ShopManagement.Api.IntegrationTests.TestSupport;

// Runs the built ShopManagement.Api.dll as a child process (research R8) and captures its output and exit code.
public sealed class ApiProcess : IDisposable
{
    private const string ListeningMarker = "Now listening on";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private static readonly string[] RemovedPrefixes = ["AdminSetup__", "ConnectionStrings__", "ASPNETCORE_", "Logging__"];

    private readonly Process _process;
    private readonly object _gate = new();
    private readonly StringBuilder _stdout = new();
    private readonly StringBuilder _stderr = new();
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly string? _ownedDatabaseFile;
    private bool _disposed;

    private ApiProcess(Process process, string? ownedDatabaseFile, string? databaseFile)
    {
        _process = process;
        _ownedDatabaseFile = ownedDatabaseFile;
        DatabaseFile = databaseFile;
    }

    // The SQLite file the process uses; null when no connection string was given.
    public string? DatabaseFile { get; }

    public string StandardOutput
    {
        get { lock (_gate) { return _stdout.ToString(); } }
    }

    public string StandardError
    {
        get { lock (_gate) { return _stderr.ToString(); } }
    }

    // Stdout and stderr together, for searches such as "no secret appears in the output".
    public string AllOutput => StandardOutput + Environment.NewLine + StandardError;

    public bool HasExited => _process.HasExited;

    // Null until the process has exited.
    public int? ExitCode => _process.HasExited ? _process.ExitCode : null;

    // True once "Now listening on" was seen in the output.
    public bool Listened => _listening.Task.IsCompleted;

    // Starts the Api. By default it uses a new temporary SQLite file as ConnectionStrings__Default;
    // pass databaseFile to choose the file, or omitConnectionString to leave the connection string out.
    // environment overrides or adds variables (for example AdminSetup__SignInName, ASPNETCORE_CONTENTROOT);
    // a null value removes the variable.
    public static ApiProcess Start(
        IReadOnlyDictionary<string, string?>? environment = null,
        IEnumerable<string>? arguments = null,
        string? databaseFile = null,
        bool omitConnectionString = false)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepositoryRoot.ApiBuildOutputDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add(RepositoryRoot.ApiDllPath);
        foreach (var argument in arguments ?? [])
        {
            startInfo.ArgumentList.Add(argument);
        }

        foreach (var key in startInfo.Environment.Keys.ToList())
        {
            if (IsRemoved(key))
            {
                startInfo.Environment.Remove(key);
            }
        }

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        startInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";

        string? ownedFile = null;
        string? usedFile = null;
        if (!omitConnectionString)
        {
            if (databaseFile is null)
            {
                ownedFile = Path.Combine(Path.GetTempPath(), $"shopmanagement-process-{Guid.NewGuid():N}.db");
                usedFile = ownedFile;
            }
            else
            {
                usedFile = databaseFile;
            }

            startInfo.Environment["ConnectionStrings__Default"] = $"Data Source={usedFile}";
        }

        foreach (var (key, value) in environment ?? new Dictionary<string, string?>())
        {
            if (value is null)
            {
                startInfo.Environment.Remove(key);
            }
            else
            {
                startInfo.Environment[key] = value;
            }
        }

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var api = new ApiProcess(process, ownedFile, usedFile);
        process.OutputDataReceived += (_, e) => api.OnLine(e.Data, api._stdout);
        process.ErrorDataReceived += (_, e) => api.OnLine(e.Data, api._stderr);
        process.Exited += (_, _) => api._exited.TrySetResult();

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("The Api process did not start.");
            }
        }
        catch
        {
            api.Dispose();
            throw;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return api;
    }

    // Waits until the process exits or prints "Now listening on" (default 60 s), then returns.
    // The process keeps running when it listened; call Stop or Dispose to kill it.
    public async Task WaitForExitOrListeningAsync(TimeSpan? timeout = null)
    {
        var finished = await Task.WhenAny(_exited.Task, _listening.Task, Task.Delay(timeout ?? DefaultTimeout));
        if (finished != _exited.Task && finished != _listening.Task)
        {
            throw new TimeoutException($"The Api neither exited nor listened within the timeout.{Environment.NewLine}{AllOutput}");
        }
    }

    // Waits for the process to exit (default 60 s) and drains the output. Kills the process and throws on timeout.
    public async Task<int> WaitForExitAsync(TimeSpan? timeout = null)
    {
        var finished = await Task.WhenAny(_exited.Task, Task.Delay(timeout ?? DefaultTimeout));
        if (finished != _exited.Task)
        {
            Stop();
            throw new TimeoutException($"The Api did not exit within the timeout.{Environment.NewLine}{AllOutput}");
        }

        // The parameterless WaitForExit also flushes the redirected output handlers.
        await Task.Run(_process.WaitForExit);
        return _process.ExitCode;
    }

    // Kills the process tree and waits for it to end.
    public void Stop()
    {
        try
        {
            if (!_process.HasExited)
            {
                try
                {
                    _process.Kill(entireProcessTree: true);
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // The process exited between the HasExited check and Kill (or is already exiting);
                    // the wait below confirms it is gone.
                }
            }

            if (_process.WaitForExit(10_000))
            {
                // The timeout overload does not flush the async output handlers; the parameterless one does.
                _process.WaitForExit();
            }
        }
        catch (InvalidOperationException)
        {
            // The process is already gone or was never started.
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        _process.Dispose();
        DeleteOwnedFiles();
    }

    private void OnLine(string? line, StringBuilder target)
    {
        if (line is null)
        {
            return;
        }

        lock (_gate)
        {
            target.AppendLine(line);
        }

        if (line.Contains(ListeningMarker, StringComparison.Ordinal))
        {
            _listening.TrySetResult();
        }
    }

    private void DeleteOwnedFiles()
    {
        if (_ownedDatabaseFile is null)
        {
            return;
        }

        foreach (var suffix in new[] { string.Empty, "-wal", "-shm", "-journal" })
        {
            try
            {
                File.Delete(_ownedDatabaseFile + suffix);
            }
            catch (IOException)
            {
                // Best effort: a leftover temp file does not affect test results.
            }
            catch (UnauthorizedAccessException)
            {
                // Best effort, as above.
            }
        }
    }

    private static bool IsRemoved(string name) =>
        name.Equals("DOTNET_ENVIRONMENT", StringComparison.OrdinalIgnoreCase)
        || RemovedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
