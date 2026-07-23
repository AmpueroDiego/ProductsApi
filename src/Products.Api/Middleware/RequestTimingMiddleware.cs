using System.Diagnostics;

namespace Products.Api.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _logFilePath;
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    public RequestTimingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        var logsDirectory = Path.Combine(environment.ContentRootPath, "logs");
        Directory.CreateDirectory(logsDirectory);
        _logFilePath = Path.Combine(logsDirectory, "requests.txt");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            await WriteLogAsync(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task WriteLogAsync(HttpContext context, long elapsedMs)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;
        var statusCode = context.Response.StatusCode;
        var line = $"{timestamp} | {method} {path} | {statusCode} | {elapsedMs}ms{Environment.NewLine}";

        await FileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, line);
        }
        finally
        {
            FileLock.Release();
        }
    }
}
