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
        catch (Exception ex){
            _logger.LogError(ex, "Unhandled exception occurred");
            var response = ApiResponse<string>.FailResponse("Internal server error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(response);
        }}}