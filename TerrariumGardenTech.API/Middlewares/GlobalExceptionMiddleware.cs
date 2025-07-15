using System.Net;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using System.Text.Json;

namespace TerrariumGardenTech.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleExceptionAsync(context, Const.UNAUTHORIZED_CODE, Const.UNAUTHORIZED_MSG, ex);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, Const.FAIL_CREATE_CODE, ex.Message, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, Const.ERROR_EXCEPTION, "Lỗi hệ thống", ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, int statusCode, string message, Exception ex)
        {
            _logger.LogError(ex, "Lỗi toàn cục");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = new BusinessResult
            {
                Status = statusCode,
                Message = message,
                Data = null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
