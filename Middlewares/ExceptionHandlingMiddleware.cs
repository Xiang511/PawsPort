using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace PawsPort.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 呼叫下一個 Middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生未預期的錯誤");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                Success = false,
                Code = "INTERNAL_SERVER_ERROR",
                Message = "伺服器內部錯誤，請稍後再試。",
                Detail = exception.Message // 正式環境建議隱藏詳細資訊
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
