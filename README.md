# DeltaLogs

This is a shared logging library for ASP.NET Core Web APIs.

## Automatic Installation (Recommended)

To allow `dotnet add package DeltaLogs` to work from anywhere without manual steps, this project is configured to automatically publish to NuGet.org via GitHub Actions.

### Prerequisite: Setup NuGet API Key (One Time)

1. Create an account on [NuGet.org](https://www.nuget.org/).
2. Go to **API Keys** -> **Create**.
3. Copy the key.
4. Go to your GitHub Repository -> **Settings** -> **Secrets and variables** -> **Actions**.
5. Click **New repository secret**.
   - Name: `NUGET_API_KEY`
   - Value: (Paste your key here)
6. Push your code to GitHub.

### How to Install in Any Project

Once setup, you can simply run:

```bash
dotnet add package DeltaLogs
```

No manual packing or file copying required!

## Manual / Local Installation

If you do not want to use NuGet.org, you can still use it locally:

### Step 1: Create the Package

```bash
dotnet pack -c Release -o LocalNuget
```

### Step 2: Install from Local Folder

```bash
dotnet nuget add source "D:\Path\To\DeltaLogs\LocalNuget" -n LocalDeltaLogs
dotnet add package DeltaLogs
```

## Configuration (Program.cs)

Add the following lines to your `Program.cs`:

```csharp
using DeltaLogs.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddDeltaLogger();

var app = builder.Build();

// 2. Use Middleware
app.UseDeltaLogger();

app.Run();
```

## GitHub Actions (Auto Publish to NuGet)
- Repository secret name must be `NUGET_API_KEY` (value from NuGet.org).
- After pushing to `main`, workflow publishes package automatically.
- If workflow fails with “Source parameter was not specified”, set the secret and re-run.
- New releases require increasing `<Version>` in `DeltaLogs.csproj` (e.g., 1.0.1).
