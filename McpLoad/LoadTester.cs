namespace McpLoad;

public class LoadTester
{
    private readonly LoadTestConfig _config;
    private readonly MetricsCollector _metrics;
    private readonly List<McpClientWorker> _workers = new();
    private readonly CancellationTokenSource _cts = new();

    public LoadTester(LoadTestConfig _config)
    {
        this._config = _config;
        _metrics = new MetricsCollector();
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== MCP Load Test ===");
        Console.WriteLine($"Target URL:          {_config.Url}");
        Console.WriteLine($"Concurrent Clients:  {_config.Clients}");
        Console.WriteLine($"Operation:           {_config.Operation}");
        if (_config.Operation.ToLower() == "call-tool")
        {
            Console.WriteLine($"Tool Name:           {_config.ToolName}");
            if (!string.IsNullOrWhiteSpace(_config.ToolArgs))
                Console.WriteLine($"Tool Arguments:      {_config.ToolArgs}");
        }
        if (_config.Duration.HasValue)
            Console.WriteLine($"Duration:            {_config.Duration}s");
        if (_config.Requests.HasValue)
            Console.WriteLine($"Total Requests:      {_config.Requests}");
        Console.WriteLine();

        // Initialize workers
        Console.WriteLine("Initializing workers...");
        for (int i = 0; i < _config.Clients; i++)
        {
            var worker = new McpClientWorker(i, _config.Url, _config, _metrics);
            try
            {
                await worker.InitializeAsync();
                _workers.Add(worker);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize worker {i}: {ex.Message}");
                throw;
            }
        }
        Console.WriteLine($"✓ {_workers.Count} workers initialized");
        Console.WriteLine();

        // Start metrics collection
        _metrics.Start();

        // Start periodic reporting
        var reportingTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_config.ReportInterval), _cts.Token);
                if (!_cts.Token.IsCancellationRequested)
                {
                    var snapshot = _metrics.GetSnapshot();
                    snapshot.Print(isFinal: false);
                }
            }
        });

        // Start workers
        Console.WriteLine("Starting load test...");
        var workerTasks = _workers.Select(w => w.RunAsync(_cts.Token)).ToList();

        // Wait for completion based on configuration
        if (_config.Duration.HasValue)
        {
            await Task.Delay(TimeSpan.FromSeconds(_config.Duration.Value));
            _cts.Cancel();
        }
        else if (_config.Requests.HasValue)
        {
            // Poll until we reach the target request count
            while (_metrics.GetSnapshot().TotalRequests < _config.Requests.Value)
            {
                await Task.Delay(100);
            }
            _cts.Cancel();
        }

        // Wait for workers to finish
        try
        {
            await Task.WhenAll(workerTasks);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }

        // Stop metrics and reporting
        _metrics.Stop();
        try
        {
            await reportingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Print final results
        var finalSnapshot = _metrics.GetSnapshot();
        finalSnapshot.Print(isFinal: true);

        // Cleanup
        foreach (var worker in _workers)
        {
            await worker.DisposeAsync();
        }
    }
}
