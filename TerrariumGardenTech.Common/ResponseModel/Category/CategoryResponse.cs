﻿namespace TerrariumGardenTech.Common.ResponseModel.Category;

public class CategoryResponse
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}