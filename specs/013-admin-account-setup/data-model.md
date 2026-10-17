# Data model: Admin account setup

**Feature:** 013-admin-account-setup
**Plan:** [plan.md](plan.md)

## Entity: `AdminAccount` (Domain/AdminAccounts/AdminAccount.cs)

The only account in the system (constitution P1; spec FR-001).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `int` | Always `1` (`AdminAccount.SingleId`) | Fixed key; the database refuses any other value (R4) |
| `SignInName` | `string` | NFC-normalized, trimmed, 1–100 code points, no control character | Letter case kept (FR-006). Matching while ignoring case is 014's job. |
| `Password` | `StoredPassword` | `IsUsable` at creation | Value object |

Construction:
- **`AdminAccount.Create(string signInName, StoredPassword password)`** is for a new account. It expects values already checked by `AdminCredentialRules` (the handler validates first). It throws `ArgumentException` if they break the rules or the password is not usable, because that would be a bug, not an expected failure.
- **`AdminAccount.Restore(string signInName, StoredPassword password)`** is for an account read from storage. It does no rule check, so stored data that was changed outside the system can still be represented and judged by the handler (FR-014).

This feature adds no update method. Spec 018 (change password) and 019 (recovery) add one.

## Value object: `StoredPassword` (Domain/AdminAccounts/StoredPassword.cs)

| Field | Type | Constraint for `IsUsable` |
|-------|------|---------------------------|
| `Algorithm` | `string` | Equals `"PBKDF2-HMAC-SHA256"` (`StoredPassword.Pbkdf2HmacSha256`) |
| `Salt` | `byte[]` | Length ≥ 16 (`MinimumSaltSize`) |
| `Iterations` | `int` | ≥ 600,000 (`MinimumIterations`) |
| `Hash` | `byte[]` | Length = 32 (`HashSize`) |

`IsUsable` is false when any constraint fails. That includes an empty algorithm, salt or hash, which is how a missing stored field arrives (FR-014). The constants are constitution P4's minimums, and the hasher uses them too. The hash-length rule is the plan's reading of FR-014's "unreadable" (research R3).

## Rules: `AdminCredentialRules` (Domain/AdminAccounts/AdminCredentialRules.cs)

Static, pure functions shared with 014, 018 and 019 (FR-015).

| Member | Behavior |
|--------|----------|
| `NormalizeSignInName(string)` | NFC, then `Trim()` (all Unicode whitespace). Returns `null` if the value cannot be normalized (research R5). |
| `NormalizePassword(string)` | NFC only; spaces kept. Returns `null` if the value cannot be normalized. |
| `ValidateSignInName(string normalized)` | Returns the violations: `Empty` (0 code points), `TooLong` (> 100), `ControlCharacter` |
| `ValidatePassword(string normalized)` | Returns the violations: `TooShort` (< 12), `TooLong` (> 128), `ControlCharacter` |
| `SignInNameMaxLength` = 100, `PasswordMinLength` = 12, `PasswordMaxLength` = 128 | Lengths are code points of the normalized value |

The violations are the enum `CredentialRuleViolation` (Domain/AdminAccounts/CredentialRuleViolation.cs): `Missing`, `Empty`, `TooShort`, `TooLong`, `ControlCharacter`, `InvalidText`. The validator sets two of them itself:
- `Missing`, for a value that is absent or `""`, before normalization;
- `InvalidText`, when `Normalize*` returns `null`.

## Persistence record: `AdminAccountRecord` (Infrastructure/Persistence/AdminAccountRecord.cs)

This is the EF Core entity for the table, used to write (`AddAsync`) and to create the schema (migration). Its property types match the `IsRequired()` columns. The repository converts a Domain `AdminAccount` into it.

| Property | C# type | Column |
|----------|---------|--------|
| `Id` | `int` | `Id` |
| `SignInName` | `string` | `SignInName` |
| `PasswordAlgorithm` | `string` | `PasswordAlgorithm` |
| `PasswordSalt` | `byte[]` | `PasswordSalt` |
| `PasswordIterations` | `int` | `PasswordIterations` |
| `PasswordHash` | `byte[]` | `PasswordHash` |

## Persistence read model: `AdminAccountRow` (Infrastructure/Persistence/AdminAccountRow.cs)

A keyless EF Core type that reads the same table and keeps damaged stored data readable (research R3). `ListAsync` reads through it. It is mapped with `ToView("AdminAccounts")` by `Infrastructure/Persistence/Configurations/AdminAccountRowConfiguration.cs`. That way it is never part of a migration and adds nothing to the schema. Its properties are nullable and not required, so EF Core reads a NULL column as `null` instead of throwing. EF Core throws for an `IsRequired()` property even when the C# type is nullable or the query casts it to a nullable type. `AppDbContext` exposes it as `DbSet<AdminAccountRow> AdminAccountRows`.

| Property | C# type | Column |
|----------|---------|--------|
| `Id` | `int` | `Id` |
| `SignInName` | `string?` | `SignInName` |
| `PasswordAlgorithm` | `string?` | `PasswordAlgorithm` |
| `PasswordSalt` | `byte[]?` | `PasswordSalt` |
| `PasswordIterations` | `int?` | `PasswordIterations` |
| `PasswordHash` | `byte[]?` | `PasswordHash` |

Converting a row to a Domain `AdminAccount`:
- A null password field becomes an empty value or 0, so `IsUsable` is false (the result is `UnusableStoredPassword`, and 019 can still recover).
- A null `SignInName` makes `ListAsync` return the failed `Result` `UnreadableStoredData`.

## Table: `AdminAccounts` (migration `AddAdminAccounts`)

Configured by `Infrastructure/Persistence/Configurations/AdminAccountRecordConfiguration.cs`.

| Column | SQLite type | Null | Notes |
|--------|-------------|------|-------|
| `Id` | INTEGER | NOT NULL | Primary key, `ValueGeneratedNever` |
| `SignInName` | TEXT | NOT NULL | `IsRequired()` |
| `PasswordAlgorithm` | TEXT | NOT NULL | `IsRequired()` |
| `PasswordSalt` | BLOB | NOT NULL | `IsRequired()` |
| `PasswordIterations` | INTEGER | NOT NULL | `IsRequired()` |
| `PasswordHash` | BLOB | NOT NULL | `IsRequired()` |

Constraint `CK_AdminAccounts_SingleRow` is `CHECK ("Id" = 1)`. Together with the primary key, it allows at most one row (FR-001). A second insert with `Id = 1` fails with `SQLITE_CONSTRAINT_PRIMARYKEY` (extended code 1555), which the repository handles as "already exists" (R4). An insert with any other `Id` fails on the CHECK.

## State transitions

```
(no stored data) --start: MigrateAsync--> (empty AdminAccounts table)                     FR-016
(empty table) --start with valid setup values--> (one AdminAccount)                       US1
(empty table) --start with missing/invalid setup values--> (empty table), exit 1          US2
(one AdminAccount) --any start--> (same AdminAccount, unchanged)                          US3, US4
(one AdminAccount, password missing/unusable) --start--> refused, exit 1                  FR-014 (019 adds recovery here)
(one row with no sign-in name) --start--> refused, exit 1                                 FR-014
(more than one row, changed outside the system) --start--> refused, exit 1                FR-014
(migration lock left by a killed start) --start--> refused after the timeout, exit 1      FR-016, FR-017
```

This feature never deletes data.
