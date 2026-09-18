using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
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

var localBaseUrl = config["LocalModel:BaseUrl"];
var localModel = config["LocalModel:Model"];

string ModelProvider = "Local";
//string ModelProvider = "AzureOpenAI";

if (ModelProvider == "AzureOpenAI")
{
    if (string.IsNullOrWhiteSpace(endpoint) ||
        string.IsNullOrWhiteSpace(apiKey) ||
        string.IsNullOrWhiteSpace(deployment))
    {
        Console.WriteLine("Missing Azure OpenAI configuration.");
        return;
    }
}
else if (ModelProvider == "Local")
{
    if (string.IsNullOrWhiteSpace(localBaseUrl) ||
        string.IsNullOrWhiteSpace(localModel))
    {
        Console.WriteLine("Missing local model configuration.");
        return;
    }
}
else
{
    Console.WriteLine($"Unknown model provider: {ModelProvider}");
    return;
}

ChatClient chatClient;

if (ModelProvider == "AzureOpenAI")
{
    var azureClient = new AzureOpenAIClient(
        new Uri(endpoint),
        new System.ClientModel.ApiKeyCredential(apiKey));

    chatClient = azureClient.GetChatClient(deployment);
}
else
{
    var localClient = new OpenAI.OpenAIClient(
        new System.ClientModel.ApiKeyCredential("lm-studio"),
        new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(localBaseUrl)
        });

    chatClient = localClient.GetChatClient(localModel);
}

var incidentPlugin = new IncidentQueryPlugin(Path.Combine("data", "incidents.sample.json"));
var policyPlugin = new PolicyPlugin("runbooks");

var tools = new[]
{
    AIFunctionFactory.Create(incidentPlugin.GetRecentIncidents),
    AIFunctionFactory.Create(policyPlugin.GetPostmortemPolicy),
    AIFunctionFactory.Create(policyPlugin.GetChangeManagementPolicy)
};

var system = $"""
You are OpsCopilot, an operations reporting assistant.

You have tools available to retrieve:
- Recent incident data
- The postmortem policy
- The change management policy

For every incident reporting request:
- Use GetRecentIncidents to retrieve the authoritative incident data.
- Use the policy tools when recommending prevention actions.
- Do not invent incidents, policies, or other data that were not returned by your tools.

Requirements:
- Produce a concise Markdown report in the exact format below.
- Ground claims in the provided incident JSON.
- For prevention actions, use the policy excerpts and cite them by tag (e.g., policy:postmortem).

Rules for incident data:
- The incident JSON returned by GetRecentIncidents is the complete and authoritative dataset for the requested period.
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

var agent = chatClient
    .AsAIAgent(
        instructions: system,
        name: "OpsCopilot",
        tools: tools);

Console.WriteLine("OpsCopilot Console (type 'exit' to quit)\n");

while (true)
{
    Console.Write("> ");
    var user = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(user)) continue;
    if (user.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        var composedUserMessage = $"""
User request:
{user}
""";
        var result = await agent.RunAsync(composedUserMessage);

        Console.WriteLine();
        Console.WriteLine(result.Text);
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine();
    }
}


