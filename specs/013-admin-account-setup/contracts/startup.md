# Contract: start-up behavior for the system operator

**Feature:** 013-admin-account-setup

## Settings

| Setting | Environment variable | Read from | Required |
|---------|----------------------|-----------|----------|
| `AdminSetup:SignInName` | `AdminSetup__SignInName` | Environment variables; user secrets in Development only | Only while no Admin account exists |
| `AdminSetup:InitialPassword` | `AdminSetup__InitialPassword` | Same | Only while no Admin account exists |
| `ConnectionStrings:Default` | `ConnectionStrings__Default` | Any configuration source that is not committed: environment variables, user secrets in Development, command-line arguments (constitution P7) | Always. It is a deployment setting of the whole system, not an Admin setup step. |

`AdminSetup:*` values in `appsettings*.json` or on the command line are ignored and count as missing (FR-003). No committed `appsettings*.json` contains an `AdminSetup` or a `ConnectionStrings` section.

Development example (values kept outside the repository):

```
dotnet user-secrets set "ConnectionStrings:Default" "Data Source=shopmanagement.db" --project src/backend/src/ShopManagement.Api
dotnet user-secrets set "AdminSetup:SignInName" "admin" --project src/backend/src/ShopManagement.Api
dotnet user-secrets set "AdminSetup:InitialPassword" "<12 to 128 characters>" --project src/backend/src/ShopManagement.Api
```

## Exit codes

| Situation | Process |
|-----------|---------|
| The Admin account was created, or it already exists with a usable password | Keeps running; "Now listening on …" is logged after the setup messages |
| No Admin account, and a setup value is missing or invalid (FR-004) | Exits with code 1 before listening on any port |
| The connection string is missing; the stored data cannot be created, upgraded or read; the migration lock is held; there is more than one Admin account; the stored password cannot be used (FR-014, FR-016) | Exits with code 1 before listening on any port |

## Log messages

All are written to the console log. `{…}` is filled by the logger. No message contains a setup value, its length, the sign-in name or the password hash (NFR-003).

| Level | When | Message |
|-------|------|---------|
| Information | The Admin account was created (FR-013) | `Admin account created at {CreatedAtUtc:O}. Remove AdminSetup:InitialPassword from the setup values now.` |
| Warning | The Admin account exists and `AdminSetup:InitialPassword` is set and not empty (FR-011) | `AdminSetup:InitialPassword is still set although the Admin account exists. Remove it from the setup values.` |
| Critical | One line per problem, for every invalid setting (FR-005) | `Admin account setup refused: {Setting} {Rule}.` For example: `Admin account setup refused: AdminSetup:InitialPassword is shorter than 12 characters.` |
| Critical | The connection string is missing | `ConnectionStrings:Default is missing. Set it in the environment or user secrets.` |
| Critical | The migration lock was not released within the timeout | `The stored data is locked by a start that did not finish. If no other start is running, delete the __EFMigrationsLock table from the database.` |
| Critical | A stored Admin account has no sign-in name (FR-014) | `The stored Admin account cannot be read.` |
| Critical | There is more than one Admin account (FR-014) | `The stored data holds more than one Admin account.` |
| Critical | The stored password cannot be used (FR-014) | `The stored Admin password cannot be used: it is missing or does not meet the required hashing parameters.` |
| Critical | Any other exception during start, such as a file that is not a database (FR-014) | `The stored data could not be prepared or read.`, with the exception attached |
| Critical | After any refusal above (FR-017) | `The system did not start.` |
