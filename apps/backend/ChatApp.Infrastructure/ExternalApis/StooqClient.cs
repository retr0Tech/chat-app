using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace ChatApp.Infrastructure.ExternalApis;

public class StooqClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StooqClient> _logger;

    public StooqClient(HttpClient httpClient, ILogger<StooqClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetStockQuoteAsync(string stockCode)
    {
        try
        {
            var url = $"https://stooq.com/q/l/?s={Uri.EscapeDataString(stockCode)}&f=sd2t2ohlcv&h&e=csv";
            var csvContent = await _httpClient.GetStringAsync(url);

            _logger.LogInformation("Stooq response for {StockCode}: {Csv}", stockCode, csvContent);

            return ParseCsv(stockCode, csvContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stock quote for {StockCode}", stockCode);
            return $"Could not retrieve quote for {stockCode.ToUpperInvariant()}";
        }
    }

    public static string ParseCsv(string stockCode, string csvContent)
    {
        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        if (!csv.Read())
            return $"Could not retrieve quote for {stockCode.ToUpperInvariant()}";

        csv.ReadHeader();

        if (!csv.Read())
            return $"Could not retrieve quote for {stockCode.ToUpperInvariant()}";

        var close = csv.GetField("Close");

        if (string.IsNullOrWhiteSpace(close) || close == "N/D")
            return $"Could not retrieve quote for {stockCode.ToUpperInvariant()}";

        var symbol = csv.GetField("Symbol") ?? stockCode;
        return $"{symbol.ToUpperInvariant()} quote is ${close} per share";
    }
}
