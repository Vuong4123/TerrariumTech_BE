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
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, Const.UNAUTHORIZED_MSG, ex);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Lỗi hệ thống", ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message, Exception ex)
        {
            _logger.LogError(ex, "Lỗi toàn cục");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;  // Set đúng mã trạng thái HTTP

            var result = new BusinessResult
            {
                Status = statusCode.GetHashCode(),  // Thêm mã trạng thái HTTP vào trong kết quả
                Message = message,
                Data = null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
