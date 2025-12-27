using System.Net;
using System.Text.Json;
using Api.Common.Responses;
using Microsoft.Extensions.Logging;

namespace Api.Common.Middleware;

public class ExceptionMiddleware{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger){
        _next = next;
        _logger = logger;}
    public async Task Invoke(HttpContext context){
        try{
            await _next(context);}
        catch (Exception ex)
{
    _logger.LogError(ex, "Unhandled exception occurred");

    int statusCode;
    string message = ex.Message;
    switch (ex){
        case ArgumentException:              
            statusCode = StatusCodes.Status400BadRequest;
            message = "BadRequest"; 
            break;
        case KeyNotFoundException:          
            statusCode = StatusCodes.Status404NotFound;
            message = "Not Found"; 
            break;
        case InvalidOperationException:
            statusCode = StatusCodes.Status409Conflict;
            message = "Conflict"; 
            break;
        default:
            statusCode = StatusCodes.Status500InternalServerError;
            message = "Internal server error"; 
            break;}
    var response = ApiResponse<string>.FailResponse(message);
    context.Response.StatusCode = statusCode;
    await context.Response.WriteAsJsonAsync(response);}}}