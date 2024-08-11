namespace Library.Middleware;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        string message;

        var exceptionType = exception.GetType();

        if (exceptionType == typeof(KeyNotFoundException))
        {
            message = exception.Message;
            status = HttpStatusCode.NotFound;
        }
        else if (exceptionType == typeof(ArgumentException))
        {
            message = exception.Message;
            status = HttpStatusCode.BadRequest;
        }
        else
        {
            message = "An unexpected error occurred.";
            status = HttpStatusCode.InternalServerError;
        }

        Log.Error(exception, "An error occurred: {ErrorMessage}", exception.Message);

        var result = JsonSerializer.Serialize(new { error = message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(result);
    }
}