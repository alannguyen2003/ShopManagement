using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ShopManagement.Application.AdminAccounts;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;
using ShopManagement.Application.Common;
using ShopManagement.Application.Tests.TestSupport;
using ShopManagement.Domain.AdminAccounts;
using ShopManagement.Domain.Common;

namespace ShopManagement.Application.Tests.AdminAccounts.SetUpAdminAccount;

public class SetUpAdminAccountHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 17, 8, 30, 0, TimeSpan.Zero);

    private readonly IDatabaseMigrator _migrator = Substitute.For<IDatabaseMigrator>();
    private readonly IAdminAccountRepository _repository = Substitute.For<IAdminAccountRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly StoredPassword _stored = new(
        StoredPassword.Pbkdf2HmacSha256, new byte[16], StoredPassword.MinimumIterations, new byte[StoredPassword.HashSize]);

    public SetUpAdminAccountHandlerTests()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());
        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AdminAccount>>([]));
        _repository.AddAsync(Arg.Any<AdminAccount>(), Arg.Any<CancellationToken>())
            .Returns(AddAdminAccountOutcome.Added);
        _hasher.Hash(Arg.Any<string>()).Returns(_stored);
    }

    private SetUpAdminAccountHandler CreateHandler() =>
        new(_migrator, _repository, _hasher, new SetUpAdminAccountValidator(), new FakeTimeProvider(Now));

    [Fact]
    public async Task HandleAsync_NoAccount_MigratesListsHashesThenAdds()
    {
        // Validation uses the real, sealed validator and cannot be observed on a substitute;
        // a valid command reaching Hash proves it ran between ListAsync and Hash.
        var token = new CancellationTokenSource().Token;

        await CreateHandler().HandleAsync(new SetUpAdminAccountCommand("admin", new string('p', 16)), token);

        Received.InOrder(() =>
        {
            _migrator.MigrateAsync(token);
            _repository.ListAsync(token);
            _hasher.Hash(Arg.Any<string>());
            _repository.AddAsync(Arg.Any<AdminAccount>(), token);
        });
    }

    [Fact]
    public async Task HandleAsync_PasswordWithSpacesAndCombiningMark_HashesNfcPasswordKeepingSpaces()
    {
        var combiningAcute = ((char)0x0301).ToString();
        var eAcute = ((char)0x00E9).ToString();
        var password = "  caf" + "e" + combiningAcute + "  pass  ";

        await CreateHandler().HandleAsync(new SetUpAdminAccountCommand("admin", password), CancellationToken.None);

        _hasher.Received(1).Hash("  caf" + eAcute + "  pass  ");
    }

    [Fact]
    public async Task HandleAsync_NameWithSpacesCaseAndCombiningMark_AddsTrimmedNfcNameKeepingCase()
    {
        var combiningAcute = ((char)0x0301).ToString();
        var eAcute = ((char)0x00E9).ToString();
        var name = "  Shop" + "e" + combiningAcute + "Admin  ";

        await CreateHandler().HandleAsync(new SetUpAdminAccountCommand(name, new string('p', 16)), CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<AdminAccount>(a => a.SignInName == "Shop" + eAcute + "Admin" && a.Password == _stored),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AccountAdded_ReturnsCreatedWithFakeTime()
    {
        var result = await CreateHandler().HandleAsync(
            new SetUpAdminAccountCommand("admin", new string('p', 16)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SetUpAdminAccountStatus.Created, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAtUtc);
    }

    [Fact]
    public async Task HandleAsync_MigrateFails_ReturnsTheFailureAndCallsNothingElse()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure(StoredDataErrors.StoredDataLocked));

        var result = await CreateHandler().HandleAsync(
            new SetUpAdminAccountCommand("admin", new string('p', 16)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(StoredDataErrors.StoredDataLocked, result.Error);
        await _repository.DidNotReceiveWithAnyArgs().ListAsync(default);
        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_ListFails_ReturnsTheFailureAndHashesAndAddsNothing()
    {
        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AdminAccount>>(StoredDataErrors.UnreadableStoredData));

        var result = await CreateHandler().HandleAsync(
            new SetUpAdminAccountCommand("admin", new string('p', 16)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(StoredDataErrors.UnreadableStoredData, result.Error);
        _hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_HashThrows_PropagatesAndDoesNotAdd()
    {
        _hasher.Hash(Arg.Any<string>()).Throws(new InvalidOperationException("hash failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().HandleAsync(
            new SetUpAdminAccountCommand("admin", new string('p', 16)), CancellationToken.None));

        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
