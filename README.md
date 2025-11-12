# MCP Load Test Tool

A quick and dirty .NET-based load testing tool for Model Context Protocol (MCP) servers using HTTP Streamable transport.

## Quick Start

### 1. Build the Project

```bash
cd McpLoad
dotnet build
```

### 2. Run the Load Test

Test against the Azure Functions MCP server:

```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp
```

This will run a 60-second load test with 10 concurrent clients calling the `hello` tool.

### 3. View Results

The tool will display real-time progress updates every 5 seconds and a final summary with:
- Total requests sent
- Success/failure rates
- Throughput (requests per second)
- Response time statistics (avg, min, max, P50, P90, P95, P99)
- Error messages (if any)

## Common Test Scenarios

### High Concurrency Test
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --clients 100
```

### Fixed Request Count Test
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --requests 1000
```

### Test Different Operations
```bash
# List tools
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --operation list-tools

# Mixed workload
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --operation mixed
```

### Call Tool with Arguments
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --tool-name save_snippet --tool-args '{"snippetname":"test","snippet":"console.log(\"hello\");"}'
```

### Test Azure Function with function key and high load
```bash
# Set the function key as an environment variable
export MCP_FUNCTION_KEY="your-function-key-here"

# Use the environment variable in the command
dotnet run -- --url https://func-api-abcdefg.azurewebsites.net/runtime/webhooks/mcp --header "x-functions-key:${MCP_FUNCTION_KEY}" --clients 10
```

## More Information

See [SPEC.md](./SPEC.md) for the complete specification.

See [McpLoad/README.md](./McpLoad/README.md) for detailed usage documentation.

## Project Structure

```
mcpload/
├── .gitignore
├── README.md                 # This file
├── SPEC.md                   # Full specification
├── mcpload.sln              # Solution file
└── McpLoad/                 # Main project directory
    ├── LoadTestConfig.cs     # Configuration model
    ├── LoadTester.cs         # Main orchestrator
    ├── McpClientWorker.cs    # Individual client worker
    ├── MetricsCollector.cs   # Metrics tracking
    ├── Program.cs            # Entry point with CLI
    ├── McpLoad.csproj        # Project file
    └── README.md             # Detailed documentation
```

## Dependencies

- .NET 9.0
- ModelContextProtocol 0.4.0-preview.3 (C# SDK)
- System.CommandLine 2.0.0

## Target Server

Designed for: [Azure Functions MCP Server](https://github.com/Azure-Samples/functions-quickstart-dotnet-azd)

## License

Sample/testing tool - use at your own risk.
