using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TerrariumGardenTech.Service.Filters;

public class AddAuthorizationHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Kiểm tra nếu action có tên 'refresh-token'
        if (operation.OperationId != null && operation.OperationId.Contains("refresh-token"))
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization", // Thêm Authorization header
                In = ParameterLocation.Header,
                Description = "Bearer token to authorize the request", // Mô tả cho token Bearer
                Required = true // Đảm bảo đây là yêu cầu
            });
    }
}