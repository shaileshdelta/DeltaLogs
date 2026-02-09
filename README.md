# DeltaLogs

**DeltaLogs** is a fully automated shared logging library for ASP.NET Core Web APIs. It provides zero-configuration request/response tracking, error monitoring, and SQL query logging out of the box.

## 🚀 Key Features

*   **Automatic Request & Response Logging**: Captures HTTP Method, URL, Body, Status Code, and Duration.
*   **Smart Error Handling**: Automatically logs exceptions with full stack traces and classifies errors by component (Controller, Service, Repository, etc.).
*   **SQL Query Monitoring**: Tracks successful and failed SQL commands.
*   **Zero Database Dependency**: Logs are stored in a structured file system (Year -> Month -> Day).
*   **Built-in Log Viewer**: Includes an API endpoint to view logs without accessing the server files.

---

## 📦 Installation

Install the package via NuGet:

```bash
dotnet add package DeltaLogs
```

Or via Package Manager Console:
```powershell
Install-Package DeltaLogs
```

---

## 🛠 Usage & Configuration (Step-by-Step)

Follow these simple steps to integrate DeltaLogs into your ASP.NET Core Web API.

### Step 1: Configure AppSettings (Optional)

By default, logs are stored in `wwwroot/Logs`. You can customize this path in `appsettings.json`.

```json
{
  "General": {
    "BasePath": "wwwroot"  // Base directory (default: wwwroot)
  },
  "LoggerPath": {
    "RelativePath": "Logs" // Sub-directory for logs (default: Logs)
  }
}
```

### Step 2: Register Services (Program.cs)

In your `Program.cs` file, add the DeltaLogs services **before** `builder.Build()`.

```csharp
using DeltaLogs.Extensions; // Import namespace

var builder = WebApplication.CreateBuilder(args);

// ... other services ...

// Add DeltaLogs services (This registers the log viewer controller)
builder.Services.AddDeltaLogger(); 

var app = builder.Build();
```

### Step 3: Add Middleware (Program.cs)

In the same `Program.cs` file, add the middleware **before** `app.Run()`.

**Important:** Add `app.UseDeltaLogger()` as early as possible in the middleware pipeline (usually after `UseHttpsRedirection` and before `MapControllers`) to ensure it captures all requests.

```csharp
// ... after app is built ...

app.UseHttpsRedirection();

// Enable DeltaLogs Middleware
app.UseDeltaLogger(); 

app.UseAuthorization();
app.MapControllers();

app.Run();
```




## 🗄️ SQL Logging (Optional)

To log SQL queries manually within your application (e.g., in your Dapper or ADO.NET repositories):

```csharp
using DeltaLogs.SqlLogging;

// Log a successful query
SqlLogger.Add("SELECT * FROM Users WHERE Id = 1");

// Log a failed query
SqlLogger.AddFailed("INSERT INTO Logs ... (failed)");
```

These SQL logs will be automatically attached to the corresponding HTTP request log entry.

---


