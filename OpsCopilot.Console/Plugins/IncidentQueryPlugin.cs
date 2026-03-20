using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace OpsCopilot.Console.Plugins;

public sealed class IncidentQueryPlugin
{
    private readonly string _jsonPath;

    public IncidentQueryPlugin(string jsonPath) => _jsonPath = jsonPath;

    public sealed record Incident(
        string Id,
        DateTime OccurredUtc,
        string Service,
        string Severity,
        string RootCause,
        string Summary
    );

    [KernelFunction, Description("Return recent incident tickets from the incident store as JSON.")]
    public string GetRecentIncidents(
        [Description("How many days back to look")] int days)
    {
        var text = File.ReadAllText(_jsonPath);
        var all = JsonSerializer.Deserialize<List<Incident>>(text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        var cutoff = DateTime.UtcNow.AddDays(-Math.Abs(days));
        var recent = all.Where(i => i.OccurredUtc >= cutoff)
                        .OrderByDescending(i => i.OccurredUtc)
                        .ToList();

        return JsonSerializer.Serialize(recent, new JsonSerializerOptions { WriteIndented = true });
    }
}

