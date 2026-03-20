using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpsCopilot.Console.Plugins;

const string PromptVersion = "v1.2";
const int DefaultLookbackDays = 30;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var endpoint = config["AzureOpenAI:Endpoint"];
var apiKey = config["AzureOpenAI:ApiKey"];
var deployment = config["AzureOpenAI:Deployment"];

if (string.IsNullOrWhiteSpace(endpoint) ||
    string.IsNullOrWhiteSpace(apiKey) ||
    string.IsNullOrWhiteSpace(deployment))
{
Console.WriteLine("Missing Azure OpenAI configuration. Set AzureOpenAI:Endpoint, AzureOpenAI:ApiKey, and AzureOpenAI:Deployment in User Secrets or environment variables.");
return;
}

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey);
var kernel = builder.Build();

var chat = kernel.GetRequiredService<IChatCompletionService>();

var incidentPlugin = new IncidentQueryPlugin(Path.Combine("data", "incidents.sample.json"));
var policyPlugin = new PolicyPlugin("runbooks");

var system = $"""
You are OpsCopilot, an operations reporting assistant.

You will be given:
- Incident data in JSON
- Policy excerpts (tagged)

Requirements:
- Produce a concise Markdown report in the exact format below.
- Ground claims in the provided incident JSON.
- For prevention actions, use the policy excerpts and cite them by tag (e.g., policy:postmortem).

Rules for incident data:
- The provided incident JSON is the complete and authoritative dataset.
- You MUST NOT invent additional incidents, root causes, severities, or counts.
- All summaries, counts, and breakdowns MUST be directly derived from the JSON.
- You MUST compute incident counts directly from the JSON array length and fields.
- If information is not present in the JSON, explicitly state that it is unknown.

Policy citation rules:
- Use policy:change-management for controls on changes (reviews, rollbacks, rollouts, config changes, certificate rotation/renewal process).
- Use policy:postmortem only when recommending postmortems or tracking postmortem action items (owners/dates) and lessons learned.

Output format (Markdown):
# OpsCopilot Report
## Executive summary
## Root cause breakdown
## Prevention actions (top 3)
## Decision log
- prompt_version: {PromptVersion}
- sources_used: incidents_json, policy:postmortem, policy:change-management
""";

Console.WriteLine("OpsCopilot Console (type 'exit' to quit)\n");

while (true)
{
    Console.Write("> ");
    var user = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(user)) continue;
    if (user.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        var incidentsJson = incidentPlugin.GetRecentIncidents(DefaultLookbackDays);
        var postmortem = policyPlugin.GetRunbook("postmortem");
        var changeMgmt = policyPlugin.GetRunbook("change-management");

        var composedUserMessage = $"""
User request:            
{user}
            
INCIDENT_DATA_JSON (authoritative, complete):
            
```
json
{incidentsJson}
```
            
Policy excerpts:
[policy:postmortem]
{postmortem}
            
[policy:change-management]
{changeMgmt}
""";

        var history = new ChatHistory(system);
        history.AddUserMessage(composedUserMessage);

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.2f
        };

        var result = await chat.GetChatMessageContentAsync(history, settings);

        Console.WriteLine();
        Console.WriteLine(result.Content);
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine();
    }
}


