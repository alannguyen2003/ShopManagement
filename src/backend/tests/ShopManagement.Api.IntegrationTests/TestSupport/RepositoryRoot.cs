namespace ShopManagement.Api.IntegrationTests.TestSupport;

// Locates the backend solution folder and the Api build output for process tests (research R8).
public static class RepositoryRoot
{
    private const string SolutionFile = "ShopManagement.slnx";
    private const string ApiProjectName = "ShopManagement.Api";

    // The folder that holds ShopManagement.slnx (src/backend).
    public static string SolutionDirectory { get; } = FindSolutionDirectory();

    // The Api project folder (src/backend/src/ShopManagement.Api), where the committed appsettings*.json live.
    public static string ApiProjectDirectory => Path.Combine(SolutionDirectory, "src", ApiProjectName);

    // The Api build output for the same configuration as the test assembly. Fails with a clear message if the dll is missing.
    public static string ApiBuildOutputDirectory
    {
        get
        {
            var directory = Path.Combine(ApiProjectDirectory, "bin", Configuration, "net10.0");
            var dll = Path.Combine(directory, ApiProjectName + ".dll");
            if (!File.Exists(dll))
            {
                throw new FileNotFoundException(
                    $"The Api build output was not found at '{dll}'. Build the solution with the '{Configuration}' configuration first (dotnet build src/backend).");
            }

            return directory;
        }
    }

    public static string ApiDllPath => Path.Combine(ApiBuildOutputDirectory, ApiProjectName + ".dll");

    // Build configuration of the running test assembly (the folder above the target framework folder: bin/<Configuration>/net10.0/).
    private static string Configuration
    {
        get
        {
            var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return new DirectoryInfo(baseDirectory).Parent?.Name ?? "Debug";
        }
    }

    private static string FindSolutionDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find '{SolutionFile}' above '{AppContext.BaseDirectory}'.");
    }
}
