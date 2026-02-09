# DeltaLogs Features & Overview

DeltaLogs is a specialized shared logging library designed for ASP.NET Core Web APIs. It provides comprehensive auditing and monitoring capabilities by automatically capturing detailed information about HTTP requests, responses, errors, and database interactions.

## Key Features

### 1. Automatic Request & Response Logging
*   **Full Cycle Tracking**: Automatically intercepts and logs incoming HTTP requests and outgoing responses using custom middleware (`RequestLoggingMiddleware`).
*   **Detailed Capture**: Records critical details such as:
    *   HTTP Method (GET, POST, PUT, DELETE, etc.)
    *   Request URL and Query Strings
    *   Request Body (payload)
    *   Response Status Code
    *   Execution Duration (in milliseconds)
    *   Client IP Address

### 2. Advanced Error Handling & Diagnostics
*   **Smart Component Classification**: Automatically identifies and categorizes the source of errors (e.g., `Controller`, `Service`, `Repository`, `Manager`, `DAL`) by analyzing the stack trace.
*   **Deep Insights**: Captures the full stack trace, specific class names, method names, and exact line numbers where exceptions occurred.
*   **Context Awareness**: Maintains a call stack history (`__DeltaLogs_CallStack`) to trace the execution path leading to an error.

### 3. Structured File-Based Logging
*   **Organized Storage**: Logs are systematically stored in a directory structure: `Year -> Month -> Daily Log File` (e.g., `2023\10\2023-10-27.log`).
*   **JSON Format**: specific log entries are saved in JSON format, making them easy to parse, search, and analyze programmatically.
*   **Daily Rotation**: Creates a new log file for each day to ensure organized record-keeping.

### 4. SQL Query Monitoring
*   **Query Tracking**: Includes a `SqlLogger` component to capture SQL commands executed during a request.
*   **Success & Failure Separation**: Distinguishes between successful SQL queries and failed ones, allowing for targeted debugging of database issues.

### 5. Built-in Log Viewer API
*   **Integrated Controller**: improved `LoggerController` provides an API endpoint to retrieve logs.
*   **Date-Based Retrieval**: Users can fetch logs for any specific date via the `/api/Logger/GetLogsByDate` endpoint.
*   **Searchable Response**: Returns structured data including status, message, and the list of log entries.

### 6. Deployment & Integration
*   **NuGet Ready**: Configured for automatic packaging and publishing to NuGet.org via GitHub Actions.
*   **Easy Setup**: Designed for plug-and-play integration with a simple `AddDeltaLogger()` service registration and `UseDeltaLogger()` middleware configuration.

## Technical Summary
*   **Framework**: .NET 8.0
*   **Type**: Middleware & Library
*   **Primary Namespace**: `DeltaLogs`
*   **Key Components**: `RequestLoggingMiddleware`, `SqlLogger`, `LoggerController`
