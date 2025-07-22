namespace TerrariumGardenTech.Common.RequestModel.Category;

public class CategoryUpdateRequest
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}