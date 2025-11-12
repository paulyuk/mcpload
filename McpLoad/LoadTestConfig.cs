namespace McpLoad;

public class LoadTestConfig
{
    public required string Url { get; set; }
    public int Clients { get; set; } = 10;
    public int? Duration { get; set; } = 60;
    public int? Requests { get; set; }
    public string Operation { get; set; } = "call-tool";
    public string ToolName { get; set; } = "hello";
    public string? ToolArgs { get; set; }
    public int ReportInterval { get; set; } = 5;
    public bool Verbose { get; set; }
    public Dictionary<string, string>? Headers { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("URL is required");

        if (Clients <= 0)
            throw new ArgumentException("Clients must be greater than 0");

        if (!Duration.HasValue && !Requests.HasValue)
            throw new ArgumentException("Either Duration or Requests must be specified");

        var validOperations = new[] { "list-tools", "call-tool", "list-resources", "list-prompts", "mixed" };
        if (!validOperations.Contains(Operation.ToLower()))
            throw new ArgumentException($"Operation must be one of: {string.Join(", ", validOperations)}");
    }
}
