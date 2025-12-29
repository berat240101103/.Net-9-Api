using System.Net;
using System.Text.Json;
using Api.Common.Responses;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Api.Common.Middlewares;

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
            int statusCode;
            string message;
            switch (ex){
                case ValidationException ve:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ve.Message;
                    break;
                case ArgumentException ae:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ae.Message;
                    break;
                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = "Not found";
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Internal server error";
                    break;}
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<string>.FailResponse(message);
            await context.Response.WriteAsJsonAsync(response);}}}
