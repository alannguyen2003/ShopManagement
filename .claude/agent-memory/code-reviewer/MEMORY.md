# code-reviewer memory

- Project convention: the single admin account is stored in a table with `Id = 1`, a CHECK constraint and `ValueGeneratedNever()`, with a separate keyless `AdminAccountRow` (mapped `ToView`) for reading damaged data. New entities must not be added to the read model, and migrations must not include it.
- Project convention: secret-carrying types (command, stored password) are sealed classes, not records, so `ToString()` cannot print values. Check this on every new type that holds a password or hash.
- Project convention: integration tests use `SqliteTestDatabase` (a shared in-memory SQLite database, one per test) with `ShopApiFactory`. The test credentials come from `TestSecrets` and are never literal.
- Recurring check: value types with arrays in their constructor (`StoredPassword`) lack null guards, while their neighbours (`AdminAccount.Create`) have them.
