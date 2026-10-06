# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                                   # output goes to ./bin (cointracking.dll + apphost)
dotnet run                                     # runs the MCP server on stdio (blocks waiting for JSON-RPC)
npx @modelcontextprotocol/inspector ./bin/cointracking   # manual testing against the built server
```

No test project, linter or formatter is configured.

Required env vars at runtime: `COINTRACKING_KEY`, `COINTRACKING_SECRET` (read through `IConfiguration`, so environment variables are the source).

## Architecture

Single-project .NET 9 console app that exposes the CoinTracking API v1 as MCP tools over stdio. Three files carry the design:

- `Program.cs` — host setup. `Host.CreateApplicationBuilder` + `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`. Registers a typed `HttpClient` for `CoinTrackingClient` with base address `https://cointracking.info/api/v1/`.
- `CoinTrackingClient.cs` — the only place that talks to the API. Every call is a `POST` with form-encoded body (`method`, `nonce`, plus non-empty params). Authentication is an HMAC-SHA512 of the exact encoded post body, keyed with the secret, sent in the `Sign` header along with `Key`. Non-success JSON (`success != 1`) is turned into an `InvalidOperationException`.
- `CoinTrackingTools.cs` — `[McpServerToolType]` class; each `[McpServerTool]` maps 1:1 to a CoinTracking method (`get_balance` → `getBalance`, etc.). The tool results are passed through `Toon.Encode` (TOON format) rather than returned as JSON.

Request flow: MCP client → tool method → `Run()` → `CoinTrackingClient.CallAsync` → HTTPS POST → JSON → TOON string back to the client.

## Things that matter when editing

- **stdout is the MCP protocol channel.** Logging is set to `Warning` and routed to stderr in `Program.cs`. Any `Console.WriteLine` to stdout will corrupt the transport.
- **Nonce:** `_nonce` is a static seeded from the current time in ms and incremented with `Interlocked.Increment`. CoinTracking requires strictly increasing nonces per key; keep it monotonic across calls.
- **Signature covers the exact body string.** If you change how `postData` is built (encoding, ordering, parameter filtering), the signature changes with it. Parameters are filtered by `IsNullOrWhiteSpace` before signing, so omitted optional params are not sent.
- **Read-only by design** (per README). Only `get*` methods are exposed; don't add write/trade methods without asking.
- `cointracking.csproj` references `appsettings*.json` with `CopyToOutputDirectory`, but neither file exists in the repo.
- Documentation (`README.md`) is in Spanish; code is in English.
