using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace cointracking;

public sealed class CoinTrackingClient(HttpClient http, IConfiguration config)
{
    private static long _nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private readonly string _key = config["COINTRACKING_KEY"] ?? throw new InvalidOperationException("COINTRACKING_KEY is not set.");
    private readonly string _secret = config["COINTRACKING_SECRET"] ?? throw new InvalidOperationException("COINTRACKING_SECRET is not set.");

    public async Task<JsonElement> CallAsync(string method, IReadOnlyDictionary<string, string?> parameters, CancellationToken ct)
    {
        var nonce = Interlocked.Increment(ref _nonce).ToString();

        var fields = new List<KeyValuePair<string, string>> { new("method", method), new("nonce", nonce) };
        fields.AddRange(parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => new KeyValuePair<string, string>(p.Key, p.Value!)));

        var postData = string.Join("&", fields.Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"));
        var sign = Convert.ToHexString(HMACSHA512.HashData(Encoding.UTF8.GetBytes(_secret), Encoding.UTF8.GetBytes(postData))).ToLowerInvariant();

        using var request = new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        request.Headers.Add("Key", _key);
        request.Headers.Add("Sign", sign);

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("success", out var success) && success.GetInt32() != 1)
        {
            var error = root.TryGetProperty("error", out var e) ? e.GetString() : body;
            throw new InvalidOperationException($"CoinTracking error: {error}");
        }

        return root.Clone();
    }
}
