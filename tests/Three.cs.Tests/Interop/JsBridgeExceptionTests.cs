using Three.Interop;

namespace Three.Tests.Interop;

public class JsBridgeExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new JsBridgeException("Bridge connection failed");

        Assert.Equal("Bridge connection failed", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner error");
        var exception = new JsBridgeException("Bridge connection failed", inner);

        Assert.Equal("Bridge connection failed", exception.Message);
        Assert.Same(inner, exception.InnerException);
    }
}
