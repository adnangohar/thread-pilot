// using System;
// using System.Net;
// using System.Net.Http;
// using System.Text.Json;
// using FluentValidation;
// using Microsoft.Extensions.Logging;

// namespace ThreadPilot.Common.Middlewares;

// public class ExceptionHandlingMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<ExceptionHandlingMiddleware> _logger;

//     public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContent context)
//     {
//         try
//         {
//             await _next(context);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "An unhandled exception occurred");
//             await HandleExceptionAsync(context, ex);
//         }
//     }

//     private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
//     {
//         context.Response.ContentType = "application/json";
//         var response = new ErrorResponse();

//         switch (exception)
//         {
//             case ValidationException valEx:
//                 response.Message = "Validation failed";
//                 response.StatusCode = (int)HttpStatusCode.BadRequest;
//                 response.Errors = valEx.Errors.Select(e => new ErrorDetail
//                 {
//                     Field = e.PropertyName,
//                     Message = e.ErrorMessage
//                 }).ToList();
//                 break;
//             case HttpRequestException httpEx:
//                 response.Message = "External service error";
//                 response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
//                 break;
//             default:
//                 response.Message = "An error occurred while processing your request";
//                 response.StatusCode = (int)HttpStatusCode.InternalServerError;
//                 break;
//         }

//         context.Response.StatusCode = response.StatusCode;
//         var jsonResponse = JsonSerializer.Serialize(response);
//         await context.Response.WriteAsync(jsonResponse);
//     }
// }

// public class ErrorResponse
// {
//     public string Message { get; set; }
//     public int StatusCode { get; set; }
//     public List<ErrorDetail> Errors { get; set; } = new();
// }

// public class ErrorDetail
// {
//     public string Field { get; set; }
//     public string Message { get; set; }
// }
