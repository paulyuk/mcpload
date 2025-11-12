# MCP Load Test Tool - Specification

## Overview
A quick and dirty command-line load testing tool for Model Context Protocol (MCP) servers, written in .NET. This tool uses the .NET MCP client SDK with HTTP streamable transport to simulate concurrent client connections and measure server performance under load.

## Goals
- Stress test MCP servers with configurable concurrent connections
- Measure response times, throughput, and error rates
- Support the streamable HTTP transport
- Simple CLI interface with real-time metrics
- Quick setup and execution

## Technology Stack
- **Runtime**: .NET 10.0+
- **MCP SDK**: [ModelContextProtocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- **NuGet Package**: [ModelContextProtocol 0.4.0-preview.3](https://www.nuget.org/packages/ModelContextProtocol/0.4.0-preview.3)
- **Transport**: HTTP Streamable 
- **Language**: C#

## Target Test Server
- **Sample Server**: [Azure Functions MCP Server](https://github.com/Azure-Samples/functions-quickstart-dotnet-azd/tree/main)
- **Endpoint**: `http://localhost:7071/runtime/webhooks/mcp`
- **Test Tool**: `hello` - Simple hello world MCP Tool that responds with a hello message
- **Available Tools**: 
  - `get_snippets` - Gets code snippets from your snippet collection
  - `save_snippet` - Saves a code snippet into your snippet collection
  - `hello` - Simple hello world MCP Tool (primary test target)

## Core Features

### 1. Connection Management
- Establish multiple concurrent MCP client connections
- Configurable number of concurrent clients (default: 100)
- Connection pooling and reuse
- Graceful connection cleanup

### 2. Load Testing Operations
- **Initialize**: Test server initialization handshake
- **List Tools**: Request available tools from the server
- **Call Tools**: Execute specific tools with parameters
- **List Resources**: Request available resources
- **List Prompts**: Request available prompts
- **Mixed Workload**: Random mix of all operations

### 3. Metrics Collection
- Total requests sent
- Successful responses
- Failed requests (with error details)
- Average response time
- Min/Max response times
- P50, P90, P95, P99 latency percentiles
- Requests per second (RPS)
- Concurrent connections active

### 4. Configuration Options
Command-line arguments:
- `--url <url>`: MCP server URL (required)
- `--clients <n>`: Number of concurrent clients (default: 10)
- `--duration <seconds>`: Test duration in seconds (default: 60)
- `--requests <n>`: Total number of requests (alternative to duration)
- `--operation <type>`: Operation type (initialize, list-tools, call-tool, list-resources, list-prompts, mixed)
- `--tool-name <name>`: Specific tool to call (for call-tool operation)
- `--tool-args <json>`: Tool arguments as JSON (for call-tool operation)
- `--report-interval <seconds>`: How often to print stats (default: 5)
- `--verbose`: Enable detailed logging

### 5. Output & Reporting
- Real-time progress with periodic stats updates
- Final summary report at completion
- Console output with color-coded results
- Optional JSON export for results

## Architecture

### Components

#### 1. MCPLoadTester (Main Orchestrator)
- Manages test lifecycle
- Coordinates worker tasks
- Aggregates metrics
- Outputs reports

#### 2. MCPClientWorker
- Individual worker instance
- Maintains MCP client connection
- Executes operations
- Reports metrics back to orchestrator

#### 3. MetricsCollector
- Thread-safe metrics aggregation
- Statistical calculations
- Percentile computation

#### 4. Configuration
- Parse command-line arguments
- Validate settings
- Provide defaults

## Implementation Plan

### Phase 1: Project Setup
1. Create .NET console application
2. Add MCP SDK NuGet package
3. Set up basic project structure

### Phase 2: Core Functionality
1. Implement MCP client wrapper with HTTP transport
2. Build metrics collection system
3. Create worker task management
4. Implement basic operations (initialize, list-tools)

### Phase 3: Enhanced Features
1. Add all operation types
2. Implement percentile calculations
3. Add real-time reporting
4. CLI argument parsing

### Phase 4: Polish
1. Error handling and resilience
2. Output formatting
3. Documentation
4. Basic testing

## Usage Examples

```bash
# Basic load test with 10 clients for 60 seconds against Azure Functions MCP server
mcpload --url http://localhost:7071/runtime/webhooks/mcp --clients 10 --duration 60

# Test hello tool calling with 50 concurrent clients
mcpload --url http://localhost:7071/runtime/webhooks/mcp --clients 50 --operation call-tool --tool-name "hello"

# Run 1000 requests as fast as possible
mcpload --url http://localhost:7071/runtime/webhooks/mcp --requests 1000 --operation list-tools

# Mixed workload test
mcpload --url http://localhost:7071/runtime/webhooks/mcp --clients 25 --duration 120 --operation mixed --verbose
```

## Success Criteria
- Successfully connects to MCP server via HTTP SSE transport
- Handles concurrent connections without crashes
- Accurately measures and reports performance metrics
- Easy to configure and run
- Provides actionable performance insights

## Out of Scope (for v1)
- WebSocket transport
- Stdio transport
- Distributed load testing (multiple machines)
- Historical result storage
- Web UI or dashboard
- Automated test scenarios/scripting
- Rate limiting controls

## Dependencies
- ModelContextProtocol 0.4.0-preview.3 (NuGet package from https://github.com/modelcontextprotocol/csharp-sdk)
- System.CommandLine (for CLI parsing)
- System.Text.Json (for JSON handling)

## Notes
- "Quick and dirty" means prioritize functionality over perfect code structure
- Focus on getting accurate metrics quickly
- Minimal error handling - fail fast and loud
- Console output is sufficient for v1
