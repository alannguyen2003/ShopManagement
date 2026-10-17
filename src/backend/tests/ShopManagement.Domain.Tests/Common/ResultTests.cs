using ShopManagement.Domain.Common;

namespace ShopManagement.Domain.Tests.Common;

public class ResultTests
{
    private static readonly Error SampleError = new("Sample.Error", "Something went wrong.");

    [Fact]
    public void Success_NoValue_IsSuccessWithNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WithError_IsFailureWithThatError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void SuccessOfT_WithValue_ExposesValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureOfT_ReadValue_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(SampleError);

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_WithNoneError_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void FieldErrors_NotProvided_DefaultsToNull()
    {
        Assert.Null(SampleError.FieldErrors);
    }

    [Fact]
    public void None_FieldErrors_IsNull()
    {
        Assert.Null(Error.None.FieldErrors);
    }

    [Fact]
    public void FieldErrors_Provided_KeepsSettingToRuleTextsMap()
    {
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Password"] = new[] { "Must be at least 12 characters.", "Must contain a digit." },
            ["Username"] = new[] { "Is required." }
        };

        var error = new Error("Sample.Invalid", "Invalid values.", fieldErrors);

        Assert.NotNull(error.FieldErrors);
        Assert.Equal(2, error.FieldErrors.Count);
        Assert.Equal(new[] { "Must be at least 12 characters.", "Must contain a digit." }, error.FieldErrors["Password"]);
        Assert.Equal(new[] { "Is required." }, error.FieldErrors["Username"]);
    }

    [Fact]
    public void Failure_WithFieldErrors_ExposesFieldErrorsOnResult()
    {
        var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Password"] = new[] { "Is required." }
        };
        var error = new Error("Sample.Invalid", "Invalid values.", fieldErrors);

        var result = Result.Failure(error);

        Assert.Same(fieldErrors, result.Error.FieldErrors);
    }
}
