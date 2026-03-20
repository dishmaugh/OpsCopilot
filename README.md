# OpsCopilot

OpsCopilot is a C# / .NET 8 console application built with Semantic Kernel and Azure OpenAI that demonstrates explicit, enterprise-style AI orchestration for operational reporting.

Instead of relying on autonomous agent behavior, the application explicitly gathers incident data and policy context, then invokes an LLM to generate a grounded, auditable Markdown report.

This project is designed as a portfolio demo for enterprise-style AI integration in existing .NET systems.

---

## What it does

Given a user request such as:

> Summarize the last 30 days of incidents by root cause and recommend 3 prevention actions.

OpsCopilot:

1. Loads recent incident data from a sample JSON file
2. Loads policy guidance from Markdown runbooks
3. Sends both as structured context to Azure OpenAI through Semantic Kernel
4. Produces a Markdown report containing:
   - Executive summary
   - Root cause breakdown
   - Top 3 prevention actions
   - Decision log

---

## Why this project matters

This is **not** a generic chatbot demo.

OpsCopilot demonstrates:

- **Semantic Kernel integration in C#**
- **Explicit orchestration** instead of unconstrained tool-calling
- **Grounded output** using authoritative incident data
- **Policy-cited recommendations** using operational runbooks
- **Deterministic structure** suitable for enterprise review
- **Simple auditability** via a decision log

The goal is to show how LLMs can be used **safely inside real business workflows**, especially in environments where trust boundaries and traceability matter.

---

## Design approach

### Explicit orchestration (intentional)
The application, not the model, decides:

- which incident data is used
- which policy sources are included
- when the model is invoked

This avoids common issues such as:

- hallucinated data
- fictional policies
- inconsistent tool usage
- hidden reasoning paths

### Short-lived task context
Each report request is built from a **fresh chat history** to avoid:

- token bloat
- cross-request contamination
- drifting behavior across multiple prompts

---

## Tech stack

- **.NET 8**
- **C#**
- **Semantic Kernel**
- **Azure OpenAI**
- **Visual Studio**
- **User Secrets** for local credential management

---

## Project structure

```text
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

> Note: In the Visual Studio solution, `data` and `runbooks` are linked into the project so they are copied to the output folder for local execution.

---

## Configuration

This project already includes a UserSecretsId, so you can set the required values directly without running user-secrets init. 
Running init is optional and would create a new local secret-store ID in your own copy of the project.

### Required settings

- `AzureOpenAI:Endpoint`
- `AzureOpenAI:ApiKey`
- `AzureOpenAI:Deployment`

### Example using User Secrets

From the project directory:

```powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE-NAME.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "AzureOpenAI:Deployment" "YOUR_DEPLOYMENT_NAME"
```

> Example placeholders only. Replace with your own Azure OpenAI resource values.

---

## Running the app

From Visual Studio:
- Set `OpsCopilot.Console` as the startup project
- Run the console app

Or from the command line:

```powershell
dotnet run --project OpsCopilot.Console
```

When prompted, enter a request such as:

```text
Summarize the last 30 days of incidents by root cause and recommend 3 prevention actions.
```

Type `exit` to quit.

---

## Example output

```md
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

---

## Current limitations

This version intentionally keeps the architecture simple:

- Incident data comes from a **sample JSON file**
- Policy guidance comes from **local Markdown runbooks**
- The model is used only for **report synthesis**, not autonomous tool execution
- The current demo assumes a **30-day lookback**

These are deliberate choices to keep the first version focused, explainable, and easy to run.

---

## Obvious next steps

Potential extensions for future versions:

- Replace sample JSON with **SQL Server** incident queries
- Add **structured logging** for request/response traces
- Add **unit tests** for plugin behavior
- Make lookback days configurable
- Add optional **RAG** for larger runbook or policy corpora

---

## Why this repo exists

I built this project to demonstrate a practical approach to enterprise AI orchestration in .NET:

- start with trusted inputs
- keep control flow explicit
- make outputs reviewable
- avoid “magic” where reliability matters

Given my background in SQL-heavy operational systems and production support, this reflects the kind of AI work I’m most interested in: integrating LLMs into real systems responsibly, where trust boundaries and auditability matter.