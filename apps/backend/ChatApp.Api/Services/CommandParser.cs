using System.Text.RegularExpressions;

namespace ChatApp.Api.Services;

public static partial class CommandParser
{
    [GeneratedRegex(@"^/stock=(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex StockCommandRegex();

    public static bool IsStockCommand(string message) =>
        StockCommandRegex().IsMatch(message.Trim());

    public static string? ExtractStockCode(string message)
    {
        var match = StockCommandRegex().Match(message.Trim());
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
