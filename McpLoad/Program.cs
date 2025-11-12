using McpLoad;

// Simple argument parser
var config = new LoadTestConfig
{
    Url = GetArg("--url") ?? "http://localhost:7071/runtime/webhooks/mcp",
    Clients = int.Parse(GetArg("--clients") ?? "10"),
    Duration = GetArgInt("--duration"),
    Requests = GetArgInt("--requests"),
    Operation = GetArg("--operation") ?? "call-tool",
    ToolName = GetArg("--tool-name") ?? "hello",
    ToolArgs = GetArg("--tool-args"),
    ReportInterval = int.Parse(GetArg("--report-interval") ?? "5"),
    Verbose = HasFlag("--verbose"),
    Headers = ParseHeaders()
};

// Set default duration if neither duration nor requests specified
if (!config.Duration.HasValue && !config.Requests.HasValue)
{
    config.Duration = 60;
}

if (HasFlag("--help") || HasFlag("-h"))
{
    PrintHelp();
    return 0;
}

try
{
    config.Validate();
}
catch (Exception ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
    Console.WriteLine();
    PrintHelp();
    return 1;
}

try
{
    var loadTester = new LoadTester(config);
    await loadTester.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (config.Verbose)
    {
        Console.WriteLine(ex.StackTrace);
    }
    return 1;
}

string? GetArg(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

int? GetArgInt(string name)
{
    var value = GetArg(name);
    return value != null ? int.Parse(value) : null;
}

bool HasFlag(string name)
{
    return Array.IndexOf(args, name) >= 0;
}

Dictionary<string, string>? ParseHeaders()
{
    var headers = new Dictionary<string, string>();
    
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--header" && i + 1 < args.Length)
        {
            var headerValue = args[i + 1];
            var colonIndex = headerValue.IndexOf(':');
            if (colonIndex > 0)
            {
                var headerName = headerValue.Substring(0, colonIndex).Trim();
                var value = headerValue.Substring(colonIndex + 1).Trim();
                headers[headerName] = value;
            }
        }
    }
    
    return headers.Count > 0 ? headers : null;
}

void PrintHelp()
{
    Console.WriteLine("MCP Load Testing Tool");
    Console.WriteLine();
    Console.WriteLine("Usage: mcpload [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --url <url>              MCP server URL (default: http://localhost:7071/runtime/webhooks/mcp)");
    Console.WriteLine("  --clients <n>            Number of concurrent clients (default: 10)");
    Console.WriteLine("  --duration <seconds>     Test duration in seconds (default: 60)");
    Console.WriteLine("  --requests <n>           Total number of requests (alternative to duration)");
    Console.WriteLine("  --operation <type>       Operation type: list-tools, call-tool, list-resources,");
    Console.WriteLine("                           list-prompts, mixed (default: call-tool)");
    Console.WriteLine("  --tool-name <name>       Tool to call for call-tool operation (default: hello)");
    Console.WriteLine("  --tool-args <json>       Tool arguments as JSON");
    Console.WriteLine("  --report-interval <sec>  Stats reporting interval (default: 5)");
    Console.WriteLine("  --header <name:value>    HTTP header (can be used multiple times)");
    Console.WriteLine("  --verbose                Enable detailed logging");
    Console.WriteLine("  --help, -h               Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  # Basic load test with 10 clients for 60 seconds");
    Console.WriteLine("  mcpload --url http://localhost:7071/runtime/webhooks/mcp");
    Console.WriteLine();
    Console.WriteLine("  # Test hello tool with 50 concurrent clients");
    Console.WriteLine("  mcpload --url http://localhost:7071/runtime/webhooks/mcp --clients 50 --tool-name hello");
    Console.WriteLine();
    Console.WriteLine("  # Run 1000 requests as fast as possible");
    Console.WriteLine("  mcpload --url http://localhost:7071/runtime/webhooks/mcp --requests 1000");
    Console.WriteLine();
    Console.WriteLine("  # Test with custom header (e.g., function key)");
    Console.WriteLine("  mcpload --url http://localhost:7071/runtime/webhooks/mcp --header \"x-functions-key:your-key-here\"");
    Console.WriteLine();
    Console.WriteLine("  # Test Azure Function with function key and high load");
    Console.WriteLine("  mcpload --url https://func-api-abcdefg.azurewebsites.net/runtime/webhooks/mcp --header \"x-functions-key:xxxxxxxxxxx==\" --clients 100");
}
