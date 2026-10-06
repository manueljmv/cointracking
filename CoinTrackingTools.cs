using System.ComponentModel;
using ModelContextProtocol.Server;
using ToonFormat;

namespace cointracking;

[McpServerToolType]
public sealed class CoinTrackingTools(CoinTrackingClient client)
{
    [McpServerTool(Name = "get_balance"), Description("Current holdings per currency with fiat and BTC values")]
    public Task<string> GetBalance(CancellationToken ct) =>
        Run("getBalance", Params(), ct);

    [McpServerTool(Name = "get_trades"), Description("List trades, optionally filtered by unix timestamp range")]
    public Task<string> GetTrades(
        [Description("Maximum number of rows")] int? limit = null,
        [Description("Start, unix timestamp")] long? start = null,
        [Description("End, unix timestamp")] long? end = null,
        CancellationToken ct = default) =>
        Run("getTrades", Params(("limit", limit), ("start", start), ("end", end)), ct);

    [McpServerTool(Name = "get_trades_by_user"), Description("List trades of a sub-user")]
    public Task<string> GetTradesByUser(
        [Description("Sub-user name")] string user,
        [Description("Maximum number of rows")] int? limit = null,
        [Description("Start, unix timestamp")] long? start = null,
        [Description("End, unix timestamp")] long? end = null,
        CancellationToken ct = default) =>
        Run("getTradesByUser", Params(("user", user), ("limit", limit), ("start", start), ("end", end)), ct);

    [McpServerTool(Name = "get_gains"), Description("Realized and unrealized gains per coin")]
    public Task<string> GetGains(
        [Description("Tax method, e.g. FIFO or LIFO")] string? taxMethod = null,
        CancellationToken ct = default) =>
        Run("getGains", Params(("tax_method", taxMethod)), ct);

    [McpServerTool(Name = "get_ledger"), Description("Ledger of all transactions with pre and post balances")]
    public Task<string> GetLedger(
        [Description("Maximum number of rows")] int? limit = null,
        [Description("Start, unix timestamp")] long? start = null,
        [Description("End, unix timestamp")] long? end = null,
        CancellationToken ct = default) =>
        Run("getLedger", Params(("limit", limit), ("start", start), ("end", end)), ct);

    [McpServerTool(Name = "get_grouped_balance"), Description("Balance grouped by exchange, group or type")]
    public Task<string> GetGroupedBalance(
        [Description("Grouping, e.g. exchange or type")] string? group = null,
        CancellationToken ct = default) =>
        Run("getGroupedBalance", Params(("group", group)), ct);

    private async Task<string> Run(string method, Dictionary<string, string?> parameters, CancellationToken ct) =>
        Toon.Encode(await client.CallAsync(method, parameters, ct));

    private static Dictionary<string, string?> Params(params (string Key, object? Value)[] items) =>
        items.ToDictionary(i => i.Key, i => i.Value?.ToString());
}
