using ShopManagement.Api.Endpoints;
using ShopManagement.Application;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;
using ShopManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(ReadSetUpAdminAccountCommand(builder.Environment));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthEndpoints();

if (!await SetUpAdminAccountAsync(app))
{
    await app.DisposeAsync();
    return 1;
}

await app.RunAsync();
return 0;

// The setup values come only from environment variables (and user secrets in Development),
// never from appsettings*.json or the command line (FR-003).
static SetUpAdminAccountCommand ReadSetUpAdminAccountCommand(IHostEnvironment environment)
{
    var setupConfiguration = new ConfigurationBuilder().AddEnvironmentVariables();
    if (environment.IsDevelopment())
    {
        setupConfiguration.AddUserSecrets<Program>(optional: true);
    }

    var values = setupConfiguration.Build();
    return new SetUpAdminAccountCommand(
        values[SetUpAdminAccountCommand.SignInNameSetting],
        values[SetUpAdminAccountCommand.InitialPasswordSetting]);
}

static async Task<bool> SetUpAdminAccountAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var command = scope.ServiceProvider.GetRequiredService<SetUpAdminAccountCommand>();
    var handler = scope.ServiceProvider.GetRequiredService<SetUpAdminAccountHandler>();

    var result = await handler.HandleAsync(command, CancellationToken.None);
    if (result.IsFailure)
    {
        // Refusal messages are added in T044 and T049.
        return false;
    }

    var outcome = result.Value;
    if (outcome.Status == SetUpAdminAccountStatus.Created)
    {
        app.Logger.LogInformation(
            "Admin account created at {CreatedAtUtc:O}. Remove AdminSetup:InitialPassword from the setup values now.",
            outcome.CreatedAtUtc);
    }

    return true;
}

public partial class Program;
