using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace OpsCopilot.Console.Plugins;

public sealed class PolicyPlugin
{
    private readonly string _runbooksDir;

    public PolicyPlugin(string runbooksDir) => _runbooksDir = runbooksDir;

    [KernelFunction, Description("Get a runbook snippet for a topic. Returns Markdown text.")]
    public string GetRunbook(
        [Description("Topic name, e.g. 'postmortem' or 'change-management'")] string topic)
    {
        var safe = topic.Trim().ToLowerInvariant();
        var path = Path.Combine(_runbooksDir, $"{safe}.md");

        if (!File.Exists(path))
            return $"(No runbook found for '{topic}'. Available: postmortem, change-management)";

        // Keep it short for demo purposes
        var lines = File.ReadAllLines(path).Take(80);
        return string.Join(Environment.NewLine, lines);
    }
}

