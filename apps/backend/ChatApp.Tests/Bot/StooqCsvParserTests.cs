using ChatApp.Infrastructure.ExternalApis;

namespace ChatApp.Tests.Bot;

public class StooqCsvParserTests
{
    [Fact]
    public void ParseCsv_ValidCsv_ReturnsQuoteMessage()
    {
        var csv = """
            Symbol,Date,Time,Open,High,Low,Close,Volume
            AAPL.US,2024-01-15,22:00:08,185.09,186.47,183.35,185.92,49410965
            """;

        var result = StooqClient.ParseCsv("aapl.us", csv);

        Assert.Equal("AAPL.US quote is $185.92 per share", result);
    }

    [Fact]
    public void ParseCsv_NoData_ReturnsErrorMessage()
    {
        var csv = """
            Symbol,Date,Time,Open,High,Low,Close,Volume
            INVALID.US,N/D,N/D,N/D,N/D,N/D,N/D,N/D
            """;

        var result = StooqClient.ParseCsv("invalid.us", csv);

        Assert.Equal("Could not retrieve quote for INVALID.US", result);
    }

    [Fact]
    public void ParseCsv_EmptyCsv_ReturnsErrorMessage()
    {
        var csv = "";

        var result = StooqClient.ParseCsv("aapl.us", csv);

        Assert.Equal("Could not retrieve quote for AAPL.US", result);
    }

    [Fact]
    public void ParseCsv_HeaderOnly_ReturnsErrorMessage()
    {
        var csv = "Symbol,Date,Time,Open,High,Low,Close,Volume\n";

        var result = StooqClient.ParseCsv("aapl.us", csv);

        Assert.Equal("Could not retrieve quote for AAPL.US", result);
    }

    [Fact]
    public void ParseCsv_MultipleRows_ReturnsFirstQuote()
    {
        var csv = """
            Symbol,Date,Time,Open,High,Low,Close,Volume
            MSFT.US,2024-01-15,22:00:08,370.20,372.40,368.80,371.50,22345678
            """;

        var result = StooqClient.ParseCsv("msft.us", csv);

        Assert.Equal("MSFT.US quote is $371.50 per share", result);
    }
}
