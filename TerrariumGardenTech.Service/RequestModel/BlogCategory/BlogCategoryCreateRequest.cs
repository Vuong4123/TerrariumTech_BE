﻿namespace TerrariumGardenTech.Service.RequestModel.BlogCategory;

public class BlogCategoryCreateRequest
{
    public string CategoryName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}