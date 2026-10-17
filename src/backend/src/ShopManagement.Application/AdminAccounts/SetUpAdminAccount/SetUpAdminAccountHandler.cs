using ShopManagement.Application.Common;
using ShopManagement.Domain.AdminAccounts;
using ShopManagement.Domain.Common;

namespace ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

public sealed class SetUpAdminAccountHandler(
    IDatabaseMigrator migrator,
    IAdminAccountRepository repository,
    IPasswordHasher hasher,
    SetUpAdminAccountValidator validator,
    TimeProvider timeProvider)
{
    public async Task<Result<SetUpAdminAccountOutcome>> HandleAsync(
        SetUpAdminAccountCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var migrated = await migrator.MigrateAsync(cancellationToken);
        if (migrated.IsFailure)
        {
            return Result.Failure<SetUpAdminAccountOutcome>(migrated.Error);
        }

        var listed = await repository.ListAsync(cancellationToken);
        if (listed.IsFailure)
        {
            return Result.Failure<SetUpAdminAccountOutcome>(listed.Error);
        }

        // Steps 3 and 4 (existing accounts) and the invalid-values refusal come in later tasks.
        _ = validator;

        var signInName = AdminCredentialRules.NormalizeSignInName(command.SignInName ?? string.Empty)!;
        var password = AdminCredentialRules.NormalizePassword(command.InitialPassword ?? string.Empty)!;

        var storedPassword = hasher.Hash(password);
        var account = AdminAccount.Create(signInName, storedPassword);

        // The AlreadyExists re-read comes in T047.
        _ = await repository.AddAsync(account, cancellationToken);
        return Result.Success(new SetUpAdminAccountOutcome(
            SetUpAdminAccountStatus.Created, timeProvider.GetUtcNow(), false));
    }
}
