using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TerrariumGardenTech.Service.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Kiểm tra nếu action có tham số kiểu IFormFile
        var fileParameter = context.ApiDescription.ParameterDescriptions
            .FirstOrDefault(p => p.ParameterDescriptor?.ParameterType == typeof(IFormFile));

        // Nếu tham số kiểu IFormFile tồn tại, thêm vào Swagger UI
        if (fileParameter != null)
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = fileParameter.Name,
                In = ParameterLocation.Query, // Sử dụng FormData cho file upload
                Description = "Upload file",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                }
            });
    }
}