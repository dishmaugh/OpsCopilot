# OpsCopilot

OpsCopilot is a C# / .NET 10 console application built with Microsoft
Agent Framework that demonstrates constrained, tool-based AI
orchestration for operational reporting.

The application exposes a small set of trusted functions for retrieving
incident data and policy context. The model decides when to invoke those
tools, while the application controls exactly which capabilities and
data sources are available. It supports both Azure OpenAI and local
OpenAI-compatible inference.

This project is designed as a portfolio demo for enterprise-style AI
integration in existing .NET systems.

------------------------------------------------------------------------

## What it does

Given a user request such as:

> Summarize the last 30 days of incidents by root cause and recommend 3
> prevention actions.

OpsCopilot:

1.  Gives the request to a Microsoft Agent Framework agent
2.  Allows the agent to invoke a constrained set of application tools:
    `GetRecentIncidents(days)`, `GetPostmortemPolicy()`, and
    `GetChangeManagementPolicy()`
3.  Retrieves authoritative incident data from sample JSON and policy
    guidance from Markdown runbooks
4.  Produces a Markdown report containing:
    -   Executive summary
    -   Root cause breakdown
    -   Top 3 prevention actions
    -   Decision log

------------------------------------------------------------------------

## Why this project matters

This is **not** a generic chatbot demo.

OpsCopilot demonstrates:

-   **Microsoft Agent Framework integration in C#**
-   **Constrained function tools** rather than unrestricted model access
-   **Model-directed tool invocation** with application-controlled trust
    boundaries
-   **Grounded output** using authoritative incident data
-   **Policy-cited recommendations** using operational runbooks
-   **Interchangeable Azure OpenAI and local OpenAI-compatible
    inference**
-   **Simple auditability** via a decision log

The goal is to show how LLMs can be used **safely inside real business
workflows**, especially in environments where trust boundaries and
traceability matter.

------------------------------------------------------------------------

## Design approach

### Constrained agent tools

The model can decide when to request information, but it can only use
capabilities explicitly exposed by the application.

The current agent has three tools:

-   `GetRecentIncidents(days)` --- returns incident data for the
    requested lookback period
-   `GetPostmortemPolicy()` --- returns the approved postmortem policy
-   `GetChangeManagementPolicy()` --- returns the approved
    change-management policy

The generic runbook lookup method is intentionally **not** exposed as an
agent tool. This prevents the model from inventing arbitrary policy or
runbook names while still allowing useful agent behavior.

### Provider-independent agent

Both Azure OpenAI and the local OpenAI-compatible provider produce a
`ChatClient`. The same Agent Framework agent, instructions, tools, and
application workflow are then used regardless of provider.

This keeps provider selection separate from the operational logic and
makes local inference practical for development and testing.

------------------------------------------------------------------------

## Tech stack

-   **.NET 10**
-   **C#**
-   **Microsoft Agent Framework**
-   **Microsoft.Extensions.AI function tools**
-   **Azure OpenAI**
-   **OpenAI-compatible local inference**
-   **LM Studio**
-   **Visual Studio**
-   **User Secrets** for local configuration

------------------------------------------------------------------------

## Project structure

``` text
OpsCopilot.Console/
├── Program.cs
├── Plugins/
│   ├── IncidentQueryPlugin.cs
│   └── PolicyPlugin.cs
├── data/
│   └── incidents.sample.json
└── runbooks/
    ├── change-management.md
    └── postmortem.md
```

> Note: In the Visual Studio solution, `data` and `runbooks` are linked
> into the project so they are copied to the output folder for local
> execution.

------------------------------------------------------------------------

## Configuration

This project already includes a `UserSecretsId`, so you can set the
required values directly without running `user-secrets init`.

The application can use either Azure OpenAI or a local OpenAI-compatible
server. Provider selection is controlled in `Program.cs`.

### Azure OpenAI settings

-   `AzureOpenAI:Endpoint`
-   `AzureOpenAI:ApiKey`
-   `AzureOpenAI:Deployment`

``` powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE-NAME.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "AzureOpenAI:Deployment" "YOUR_DEPLOYMENT_NAME"
```

### Local model settings

-   `LocalModel:BaseUrl`
-   `LocalModel:Model`

Example for an LM Studio server:

``` powershell
dotnet user-secrets set "LocalModel:BaseUrl" "http://localhost:1234/v1/"
dotnet user-secrets set "LocalModel:Model" "YOUR_MODEL_IDENTIFIER"
```

> The local model must support OpenAI-compatible chat completions. Agent
> tool invocation also requires a model/runtime combination with
> compatible tool-calling support.

> Example placeholders only. Replace them with values for your own Azure
> resource or local model server.

------------------------------------------------------------------------

## Running the app

From Visual Studio: - Set `OpsCopilot.Console` as the startup project -
Select the desired model provider in `Program.cs` - If using a local
provider, start the OpenAI-compatible model server - Run the console app

Or from the command line:

``` powershell
dotnet run --project OpsCopilot.Console
```

When prompted, enter a request such as:

``` text
Summarize the last 30 days of incidents by root cause and recommend 3 prevention actions.
```

Type `exit` to quit.

------------------------------------------------------------------------

## Example output

``` md
# OpsCopilot Report

## Executive summary
In the past 30 days, two incidents were reported:
- **INC-1001**: A Sev2 incident in the Payments service caused by configuration drift due to an unreviewed configuration change.
- **INC-1002**: A Sev1 incident in the Auth service caused by an expired certificate, leading to login failures.

## Root cause breakdown
- **Configuration drift**: 1 incident (50%)
- **Expired certificate**: 1 incident (50%)

## Prevention actions (top 3)
1. **Enforce peer review and rollback plans for configuration changes**: Implement stricter adherence to change management policies, including mandatory peer reviews and rollback plans for all production changes. (policy:change-management)
2. **Automate certificate rotation and renewal**: Establish automated processes to monitor and renew certificates before expiration to avoid service disruption. (policy:change-management)
3. **Conduct postmortems for both incidents**: Document the impact, timeline, root cause, and contributing factors for each incident. Assign action items with owners and due dates to ensure follow-through. (policy:postmortem)

## Decision log
- prompt_version: v1.2
- sources_used: incidents_json, policy:postmortem, policy:change-management
```

------------------------------------------------------------------------

## Current limitations

This version intentionally keeps the data layer simple:

-   Incident data comes from a **sample JSON file**
-   Policy guidance comes from **local Markdown runbooks**
-   Tool-calling reliability depends on the selected model and inference
    runtime
-   Small local models may still produce unsupported statements even
    when tool invocation succeeds
-   The application does not currently validate every generated factual
    statement against the retrieved source data

These constraints keep the demo focused while making the agent/tool
boundary visible and testable.

------------------------------------------------------------------------

## Obvious next steps

Potential extensions for future versions:

-   Replace sample JSON with **SQL Server** incident queries
-   Add **structured logging** for agent and tool traces
-   Add **unit tests** for tool behavior
-   Add output validation for stronger grounding guarantees
-   Add optional **RAG** for larger runbook or policy corpora

------------------------------------------------------------------------

## Why this repo exists

I built this project to demonstrate a practical approach to enterprise
AI orchestration in .NET:

-   expose only trusted application capabilities
-   let the agent use those capabilities without giving it unrestricted
    access
-   keep data retrieval deterministic
-   make outputs reviewable
-   support both hosted and local inference

Given my background in SQL-heavy operational systems and production
support, this reflects the kind of AI work I'm most interested in:
integrating LLMs into real systems responsibly, where trust boundaries
and auditability matter.
