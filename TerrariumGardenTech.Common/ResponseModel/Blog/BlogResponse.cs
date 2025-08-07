namespace TerrariumGardenTech.Common.ResponseModel.Blog;

public class BlogResponse
{
    public int BlogId { get; set; }

    public int UserId { get; set; }

    public int BlogCategoryId { get; set; }
    public string UrlImage { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string bodyHTML { get; set; } = string.Empty;
    public bool IsFeatured { get; set; } = false; // Thêm trường IsFeatured
    public string Content { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}