using System.ComponentModel;

namespace OpsCopilot.Console.Plugins;

public sealed class PolicyPlugin
{
    private readonly string _runbooksDir;

    public PolicyPlugin(string runbooksDir) => _runbooksDir = runbooksDir;

    [Description("Get a runbook snippet for a topic. Returns Markdown text.")]
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

    [Description("Return the postmortem policy used for incident reviews and follow-up actions.")]
    public string GetPostmortemPolicy()
    {
        System.Console.WriteLine("[TOOL] GetPostmortemPolicy()");
        return GetRunbook("postmortem");
    }
    //public string GetPostmortemPolicy()
    //    => GetRunbook("postmortem");

    [Description("Return the change management policy used for production changes, reviews, rollbacks, configuration changes, and certificate rotation.")]
    public string GetChangeManagementPolicy()
    {
        System.Console.WriteLine("[TOOL] GetChangeManagementPolicy()");
        return GetRunbook("change-management");
    }
    //public string GetChangeManagementPolicy()
    //    => GetRunbook("change-management");
}

