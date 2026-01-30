# DeltaLogs

This is a shared logging library for ASP.NET Core Web APIs.

## Easy Integration via NuGet

You can install this logger as a NuGet package, which is the easiest way to reuse it across projects.

### Step 1: Create the Package (One Time)

Open a terminal in the `DeltaLogs` directory and run:

```bash
dotnet pack -c Release -o LocalNuget
```

This will create a `.nupkg` file in a `LocalNuget` folder.

### Step 2: Install the Package

In your other API projects, you can install it from your local folder:

```bash
dotnet nuget add source "Path\To\DeltaLogger\DeltaLogs\LocalNuget" -n LocalDeltaLogs
dotnet add package DeltaLogs
```

### Step 3: Configure Program.cs

Add the following two lines to your `Program.cs`:

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

### Optional: Configuration

By default, logs are saved to `wwwroot/Logs`. If you want to change this, add to `appsettings.json`:

```json
{
  "General": { "BasePath": "wwwroot" },
  "LoggerPath": { "RelativePath": "Logs/" }
}
```
