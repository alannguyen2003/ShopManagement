# Contract: layer signatures

**Feature:** 013-admin-account-setup

No HTTP endpoint is added. These are the public types between layers. Namespaces follow folders.

## Domain

```csharp
// Domain/Common/Result.cs (modify): optional per-field messages on Error.
// Error.None and every existing use stay valid; ResultTests keep passing.
public sealed record Error(
    string Code,
    string Message,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? FieldErrors = null);

// Domain/AdminAccounts/
public sealed class AdminAccount
{
    public const int SingleId = 1;
    public int Id { get; }
    public string SignInName { get; }
    public StoredPassword Password { get; }
    public static AdminAccount Create(string signInName, StoredPassword password);  // new account; throws on a bug
    public static AdminAccount Restore(string signInName, StoredPassword password); // from storage; no rule check
}

public sealed class StoredPassword
{
    public const string Pbkdf2HmacSha256 = "PBKDF2-HMAC-SHA256";
    public const int MinimumSaltSize = 16;
    public const int MinimumIterations = 600_000;
    public const int HashSize = 32;
    public StoredPassword(string algorithm, byte[] salt, int iterations, byte[] hash);
    public string Algorithm { get; }
    public byte[] Salt { get; }
    public int Iterations { get; }
    public byte[] Hash { get; }
    public bool IsUsable { get; }
}

public enum CredentialRuleViolation { Missing, Empty, TooShort, TooLong, ControlCharacter, InvalidText }

public static class AdminCredentialRules
{
    public const int SignInNameMaxLength = 100;
    public const int PasswordMinLength = 12;
    public const int PasswordMaxLength = 128;
    // Return null when the value cannot be normalized (unpaired surrogate; research R5).
    public static string? NormalizeSignInName(string value);
    public static string? NormalizePassword(string value);
    public static IReadOnlyList<CredentialRuleViolation> ValidateSignInName(string normalized);
    public static IReadOnlyList<CredentialRuleViolation> ValidatePassword(string normalized);
}
```

## Application

```csharp
// Application/Common/IDatabaseMigrator.cs
public interface IDatabaseMigrator
{
    // Creates the stored data if it does not exist and applies pending migrations (FR-016).
    // Failure: ConnectionStringMissing, StoredDataLocked (research R6).
    Task<Result> MigrateAsync(CancellationToken cancellationToken);
}

// Application/Common/StoredDataErrors.cs
public static class StoredDataErrors
{
    public static readonly Error ConnectionStringMissing; // "StoredData.ConnectionStringMissing"
    public static readonly Error StoredDataLocked;        // "StoredData.Locked"
    public static readonly Error UnreadableStoredData;    // "StoredData.Unreadable"
}

// Application/AdminAccounts/
public interface IPasswordHasher
{
    // normalizedPassword is already NFC (AdminCredentialRules.NormalizePassword).
    StoredPassword Hash(string normalizedPassword);
}

public enum AddAdminAccountOutcome { Added, AlreadyExists }

public interface IAdminAccountRepository
{
    // Failure UnreadableStoredData when a row has no sign-in name (research R3).
    Task<Result<IReadOnlyList<AdminAccount>>> ListAsync(CancellationToken cancellationToken);
    // AlreadyExists when another start inserted the row first (research R4).
    Task<AddAdminAccountOutcome> AddAsync(AdminAccount account, CancellationToken cancellationToken);
}

// Application/AdminAccounts/SetUpAdminAccount/
// A sealed class, not a record: its ToString() must never print the values (research R2).
public sealed class SetUpAdminAccountCommand(string? signInName, string? initialPassword)
{
    public const string SignInNameSetting = "AdminSetup:SignInName";
    public const string InitialPasswordSetting = "AdminSetup:InitialPassword";
    public string? SignInName { get; } = signInName;
    public string? InitialPassword { get; } = initialPassword;
}

public enum SetUpAdminAccountStatus { Created, AlreadyExists }

public sealed record SetUpAdminAccountOutcome(
    SetUpAdminAccountStatus Status,
    DateTimeOffset? CreatedAtUtc,          // set when Status == Created
    bool InitialPasswordStillPresent);     // set when Status == AlreadyExists (FR-011)

public static class AdminSetupErrors
{
    // Code "AdminSetup.InvalidSetupValues"; FieldErrors: setting name → rule texts (FR-005).
    public static Error InvalidSetupValues(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors);
    public static readonly Error MultipleAdminAccounts;   // "AdminSetup.MultipleAdminAccounts"
    public static readonly Error UnusableStoredPassword;  // "AdminSetup.UnusableStoredPassword"
}

public sealed class SetUpAdminAccountValidator
{
    // Every problem of both settings, keyed by setting name (FR-005, edge case "both invalid").
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Validate(SetUpAdminAccountCommand command);
}

public sealed class SetUpAdminAccountHandler(
    IDatabaseMigrator migrator,
    IAdminAccountRepository repository,
    IPasswordHasher hasher,
    SetUpAdminAccountValidator validator,
    TimeProvider timeProvider)
{
    public Task<Result<SetUpAdminAccountOutcome>> HandleAsync(
        SetUpAdminAccountCommand command, CancellationToken cancellationToken);
}
```

`TimeProvider` (BCL) stands in for the architecture's `IClock` example; no new interface is needed. Unit tests use a hand-written `FakeTimeProvider` subclass that overrides `GetUtcNow()`. The `Microsoft.Extensions.TimeProvider.Testing` package is not on the allowlist.

### `SetUpAdminAccountHandler.HandleAsync` order

1. `migrator.MigrateAsync`. A failure is returned as is (FR-016, FR-014).
2. `repository.ListAsync`. A failure is returned as is (FR-014).
3. More than one account → `Failure(MultipleAdminAccounts)` (FR-014).
4. One account:
   - **[019 hook A]** Spec 019 applies a recovery here, before the next check (019 FR-002).
   - `!Password.IsUsable` → `Failure(UnusableStoredPassword)` (FR-014).
   - Otherwise → `Success(AlreadyExists, InitialPasswordStillPresent: !string.IsNullOrEmpty(command.InitialPassword))`. The setup values are not validated (FR-009, FR-010, US3 #3).
   - **[019 hook B]** Spec 019 adds its "new password still present" warning flag next to `InitialPasswordStillPresent` (019 FR-017).
5. No account:
   - **[019 hook C]** Spec 019 refuses here when the reset indication is present, before validating or creating anything (019 FR-016).
   - `validator.Validate(command)`. Any problem → `Failure(InvalidSetupValues(...))`, and nothing is hashed or stored (FR-004, FR-005).
   - Otherwise normalize both values, call `hasher.Hash(password)`, then `AdminAccount.Create` and `repository.AddAsync`.
   - `Added` → `Success(Created, CreatedAtUtc: timeProvider.GetUtcNow())` (FR-013).
   - `AlreadyExists` (a concurrent start won, research R4) → go back to step 2 once. The result is then that of step 3 or 4.

### Validator rule texts (FR-005)

Per setting. No text contains the value or its length.

| Violation | `AdminSetup:SignInName` | `AdminSetup:InitialPassword` |
|-----------|-------------------------|------------------------------|
| Missing (absent or `""`) | "is missing" | "is missing" |
| Empty (only whitespace) | "is empty" | — (whitespace is a valid password) |
| TooShort | — | "is shorter than 12 characters" |
| TooLong | "is longer than 100 characters" | "is longer than 128 characters" |
| ControlCharacter | "contains a control character" | "contains a control character" |
| InvalidText | "is not valid Unicode text" | "is not valid Unicode text" |

## Infrastructure

```csharp
// Infrastructure/DependencyInjection.cs (modify)
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration);
// Registers:
// - AppDbContext: SQLite; ConnectionStrings:Default is read when the context is resolved.
// - IDatabaseMigrator → DatabaseMigrator, which checks the connection string first and
//   returns ConnectionStringMissing when it is absent.
// - IAdminAccountRepository → AdminAccountRepository
// - IPasswordHasher → Pbkdf2PasswordHasher
// - TimeProvider.System

public sealed class Pbkdf2PasswordHasher : IPasswordHasher;            // Infrastructure/Security/
public sealed class DatabaseMigrator : IDatabaseMigrator               // Infrastructure/Persistence/
{
    public static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(60);
}
public sealed class AdminAccountRepository : IAdminAccountRepository;   // Infrastructure/Persistence/Repositories/
public sealed class AdminAccountRecord;                                 // Infrastructure/Persistence/ (data-model.md)
public sealed class AppDbContext : DbContext                            // Infrastructure/Persistence/
{
    public DbSet<AdminAccountRecord> AdminAccounts { get; }
}
```

## Api

Only `Program.cs` (the composition root) changes. See [startup.md](startup.md) for what the system operator sees.

```csharp
// Program.cs, outline
var builder = WebApplication.CreateBuilder(args);
// ... existing registrations ...
builder.Services.AddSingleton(ReadSetUpAdminAccountCommand(builder.Environment)); // env vars (+ user secrets in Development) only (R2)
var app = builder.Build();
// ... existing middleware and endpoints ...
if (!await SetUpAdminAccountAsync(app))   // scope → command + handler → log outcome (R1, R7); catch (Exception) → log, false
{
    await app.DisposeAsync();               // flushes the console log
    return 1;
}
await app.RunAsync();
return 0;
```
