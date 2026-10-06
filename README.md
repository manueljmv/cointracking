# CoinTracking MCP Server

Servidor MCP (stdio) de solo lectura para la API v1 de CoinTracking.

## Configuración

Variables de entorno (API key con permiso de lectura, creada en CoinTracking):

```bash
export COINTRACKING_KEY="..."
export COINTRACKING_SECRET="..."
```

## Herramientas

| Herramienta | Método API | Parámetros |
|---|---|---|
| `get_balance` | getBalance | — |
| `get_trades` | getTrades | limit, start, end |
| `get_trades_by_user` | getTradesByUser | user, limit, start, end |
| `get_gains` | getGains | tax_method |
| `get_ledger` | getLedger | limit, start, end |
| `get_grouped_balance` | getGroupedBalance | group |

Las respuestas se devuelven en formato TOON.

## Ejecución

```bash
dotnet build
npx @modelcontextprotocol/inspector ./bin/cointracking
```
