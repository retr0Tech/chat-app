using ChatApp.Api.Services;

namespace ChatApp.Tests.Services;

public class CommandParserTests
{
    [Theory]
    [InlineData("/stock=aapl.us", true)]
    [InlineData("/stock=GOOGL", true)]
    [InlineData("/stock=msft.us", true)]
    [InlineData("/Stock=aapl.us", true)]
    [InlineData("Hello world", false)]
    [InlineData("/stockaapl.us", false)]
    [InlineData("", false)]
    [InlineData("/stock=", false)]
    public void IsStockCommand_DetectsCorrectly(string message, bool expected)
    {
        // The regex requires at least one char after =, so "/stock=" with nothing after is false
        var result = CommandParser.IsStockCommand(message);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/stock=aapl.us", "aapl.us")]
    [InlineData("/stock=GOOGL", "GOOGL")]
    [InlineData("  /stock=msft.us  ", "msft.us")]
    public void ExtractStockCode_ReturnsCorrectCode(string message, string expected)
    {
        var result = CommandParser.ExtractStockCode(message);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractStockCode_ReturnsNull_ForNonCommand()
    {
        var result = CommandParser.ExtractStockCode("Hello world");
        Assert.Null(result);
    }
}
