# MCP Load Test Tool

A quick and dirty command-line load testing tool for Model Context Protocol (MCP) servers, written in .NET.

## Features

- Concurrent client connections with configurable worker count
- Support for HTTP Streamable transport
- Multiple operation types: list-tools, call-tool, list-resources, list-prompts, and mixed workload
- Real-time metrics reporting with configurable intervals
- Detailed performance statistics including percentiles (P50, P90, P95, P99)
- Duration-based or request-count-based test execution

## Requirements

- .NET 9.0 SDK or later
- MCP server with HTTP Streamable transport support

## Building

```bash
cd McpLoad
dotnet build
```

## Usage

```bash
dotnet run -- [options]
```

### Options

- `--url <url>` - MCP server URL (default: http://localhost:7071/runtime/webhooks/mcp)
- `--clients <n>` - Number of concurrent clients (default: 10)
- `--duration <seconds>` - Test duration in seconds (default: 60)
- `--requests <n>` - Total number of requests (alternative to duration)
- `--operation <type>` - Operation type: list-tools, call-tool, list-resources, list-prompts, mixed (default: call-tool)
- `--tool-name <name>` - Tool to call for call-tool operation (default: hello)
- `--tool-args <json>` - Tool arguments as JSON
- `--report-interval <sec>` - Stats reporting interval in seconds (default: 5)
- `--verbose` - Enable detailed logging
- `--help, -h` - Show help message

## Examples

### Basic load test with default settings (10 clients, 60 seconds, call "hello" tool)
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp
```

### Test with 50 concurrent clients for 120 seconds
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --clients 50 --duration 120
```

### Run 1000 requests as fast as possible
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --requests 1000
```

### Test listing tools operation
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --operation list-tools
```

### Test calling a tool with arguments
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --tool-name save_snippet --tool-args '{"snippetname":"test","snippet":"code here"}'
```

### Mixed workload test with verbose logging
```bash
dotnet run -- --url http://localhost:7071/runtime/webhooks/mcp --operation mixed --clients 25 --verbose
```

## Target Server

This tool was designed to test the Azure Functions MCP server:
- Repository: https://github.com/Azure-Samples/functions-quickstart-dotnet-azd
- Default endpoint: http://localhost:7071/runtime/webhooks/mcp

The server provides these tools:
- `hello` - Simple hello world MCP Tool
- `get_snippets` - Gets code snippets from collection
- `save_snippet` - Saves a code snippet

## Metrics Reported

The tool reports the following metrics during and after the test:

- **Elapsed Time** - Total time elapsed
- **Total Requests** - Number of requests sent
- **Successful/Failed** - Request success rate with percentages
- **Requests/sec** - Throughput
- **Response Times** - Average, Min, Max, P50, P90, P95, P99 (in milliseconds)
- **Recent Errors** - Up to 10 most recent error messages

## Technical Details

- **SDK**: ModelContextProtocol 0.4.0-preview.3
- **Transport**: HTTP Streamable
- **Concurrency**: Task-based async workers
- **Metrics**: Thread-safe concurrent collections

## Notes

This is a "quick and dirty" tool focused on functionality over polish. It:
- Prioritizes getting accurate metrics quickly
- Has minimal error handling - fails fast and loud
- Uses console output for all reporting
- Is designed for testing, not production use

## License

This is a sample/testing tool. Use at your own risk.
