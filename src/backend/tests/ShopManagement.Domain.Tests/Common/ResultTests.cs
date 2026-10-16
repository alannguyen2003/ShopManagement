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
}
