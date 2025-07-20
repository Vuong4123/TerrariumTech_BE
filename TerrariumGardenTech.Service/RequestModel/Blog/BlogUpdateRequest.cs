using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TerrariumGardenTech.Service.RequestModel.Blog;

public class BlogUpdateRequest
{
    public int BlogId { get; set; }

    public int BlogCategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    public string? bodyHTML { get; set; }

    public string Status { get; set; } = string.Empty;
    [FromForm]
    public IFormFile? ImageFile { get; set; } // Thay UrlImage bằng file upload
}