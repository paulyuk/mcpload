using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpLoad;

public class McpClientWorker : IAsyncDisposable
{
    private readonly string _url;
    private readonly LoadTestConfig _config;
    private readonly MetricsCollector _metrics;
    private readonly int _workerId;
    private McpClient? _client;

    public McpClientWorker(int workerId, string url, LoadTestConfig config, MetricsCollector metrics)
    {
        _workerId = workerId;
        _url = url;
        _config = config;
        _metrics = metrics;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Create HTTP transport with streamable HTTP
            var transportOptions = new HttpClientTransportOptions
            {
                Endpoint = new Uri(_url),
                TransportMode = HttpTransportMode.StreamableHttp
            };

            // Add custom headers if provided
            if (_config.Headers != null && _config.Headers.Count > 0)
            {
                transportOptions.AdditionalHeaders = _config.Headers;
            }

            var transport = new HttpClientTransport(transportOptions);

            // Create and connect MCP client (this automatically initializes)
            _client = await McpClient.CreateAsync(transport);

            if (_config.Verbose)
                Console.WriteLine($"[Worker {_workerId}] Initialized connection to {_url}");
        }
        catch (Exception ex)
        {
            if (_config.Verbose)
                Console.WriteLine($"[Worker {_workerId}] Failed to initialize: {ex.Message}");
            throw;
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (_client == null)
            throw new InvalidOperationException("Client not initialized");

        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteOperationAsync();
        }
    }

    private async Task ExecuteOperationAsync()
    {
        var sw = Stopwatch.StartNew();
        bool success = false;
        string? error = null;

        try
        {
            switch (_config.Operation.ToLower())
            {
                case "list-tools":
                    await ListToolsAsync();
                    break;
                case "call-tool":
                    await CallToolAsync();
                    break;
                case "list-resources":
                    await ListResourcesAsync();
                    break;
                case "list-prompts":
                    await ListPromptsAsync();
                    break;
                case "mixed":
                    await ExecuteMixedOperationAsync();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operation: {_config.Operation}");
            }
            success = true;
        }
        catch (Exception ex)
        {
            error = $"Worker {_workerId}: {ex.Message}";
            if (_config.Verbose)
                Console.WriteLine($"[Worker {_workerId}] Error: {ex.Message}");
        }
        finally
        {
            sw.Stop();
            _metrics.RecordRequest(sw.ElapsedMilliseconds, success, error);
        }
    }

    private async Task ListToolsAsync()
    {
        if (_client == null) return;
        
        var tools = await _client.ListToolsAsync();
        
        if (_config.Verbose)
            Console.WriteLine($"[Worker {_workerId}] Listed {tools.Count} tools");
    }

    private async Task CallToolAsync()
    {
        if (_client == null) return;
        
        var toolArgs = string.IsNullOrWhiteSpace(_config.ToolArgs) 
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(_config.ToolArgs);

        var result = await _client.CallToolAsync(_config.ToolName, toolArgs);
        
        if (_config.Verbose)
            Console.WriteLine($"[Worker {_workerId}] Called tool '{_config.ToolName}' - IsError: {result.IsError}");
    }

    private async Task ListResourcesAsync()
    {
        if (_client == null) return;
        
        var resources = await _client.ListResourcesAsync();
        
        if (_config.Verbose)
            Console.WriteLine($"[Worker {_workerId}] Listed {resources.Count} resources");
    }

    private async Task ListPromptsAsync()
    {
        if (_client == null) return;
        
        var prompts = await _client.ListPromptsAsync();
        
        if (_config.Verbose)
            Console.WriteLine($"[Worker {_workerId}] Listed {prompts.Count} prompts");
    }

    private async Task ExecuteMixedOperationAsync()
    {
        var operations = new[] { "list-tools", "call-tool", "list-resources", "list-prompts" };
        var random = new Random();
        var selectedOp = operations[random.Next(operations.Length)];

        var previousOp = _config.Operation;
        _config.Operation = selectedOp;
        
        try
        {
            await ExecuteOperationAsync();
        }
        finally
        {
            _config.Operation = previousOp;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
