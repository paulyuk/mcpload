using System.Collections.Concurrent;
using System.Diagnostics;

namespace McpLoad;

public class MetricsCollector
{
    private readonly ConcurrentBag<long> _responseTimes = new();
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private readonly Stopwatch _stopwatch = new();
    private readonly ConcurrentBag<string> _errors = new();

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public void RecordRequest(long elapsedMs, bool success, string? error = null)
    {
        Interlocked.Increment(ref _totalRequests);
        
        if (success)
        {
            Interlocked.Increment(ref _successfulRequests);
            _responseTimes.Add(elapsedMs);
        }
        else
        {
            Interlocked.Increment(ref _failedRequests);
            if (error != null)
                _errors.Add(error);
        }
    }

    public MetricsSnapshot GetSnapshot()
    {
        var times = _responseTimes.ToArray();
        Array.Sort(times);

        return new MetricsSnapshot
        {
            TotalRequests = _totalRequests,
            SuccessfulRequests = _successfulRequests,
            FailedRequests = _failedRequests,
            ElapsedSeconds = _stopwatch.Elapsed.TotalSeconds,
            AverageResponseTime = times.Length > 0 ? times.Average() : 0,
            MinResponseTime = times.Length > 0 ? times.Min() : 0,
            MaxResponseTime = times.Length > 0 ? times.Max() : 0,
            P50 = GetPercentile(times, 0.50),
            P90 = GetPercentile(times, 0.90),
            P95 = GetPercentile(times, 0.95),
            P99 = GetPercentile(times, 0.99),
            RequestsPerSecond = _stopwatch.Elapsed.TotalSeconds > 0 
                ? _totalRequests / _stopwatch.Elapsed.TotalSeconds 
                : 0,
            Errors = _errors.Take(10).ToArray() // Only show first 10 errors
        };
    }

    private static double GetPercentile(long[] sortedTimes, double percentile)
    {
        if (sortedTimes.Length == 0) return 0;
        
        int index = (int)Math.Ceiling(percentile * sortedTimes.Length) - 1;
        index = Math.Max(0, Math.Min(sortedTimes.Length - 1, index));
        
        return sortedTimes[index];
    }
}

public class MetricsSnapshot
{
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public double ElapsedSeconds { get; set; }
    public double AverageResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public double P50 { get; set; }
    public double P90 { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
    public double RequestsPerSecond { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    public void Print(bool isFinal = false)
    {
        Console.WriteLine();
        Console.WriteLine(isFinal ? "=== FINAL RESULTS ===" : "=== PROGRESS UPDATE ===");
        Console.WriteLine($"Elapsed Time:        {ElapsedSeconds:F2}s");
        Console.WriteLine($"Total Requests:      {TotalRequests}");
        Console.WriteLine($"Successful:          {SuccessfulRequests} ({(TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0):F1}%)");
        Console.WriteLine($"Failed:              {FailedRequests} ({(TotalRequests > 0 ? (double)FailedRequests / TotalRequests * 100 : 0):F1}%)");
        Console.WriteLine($"Requests/sec:        {RequestsPerSecond:F2}");
        Console.WriteLine();
        Console.WriteLine("Response Times (ms):");
        Console.WriteLine($"  Average:           {AverageResponseTime:F2}");
        Console.WriteLine($"  Min:               {MinResponseTime:F2}");
        Console.WriteLine($"  Max:               {MaxResponseTime:F2}");
        Console.WriteLine($"  P50:               {P50:F2}");
        Console.WriteLine($"  P90:               {P90:F2}");
        Console.WriteLine($"  P95:               {P95:F2}");
        Console.WriteLine($"  P99:               {P99:F2}");

        if (Errors.Length > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Recent Errors (showing {Errors.Length}):");
            foreach (var error in Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        Console.WriteLine();
    }
}
