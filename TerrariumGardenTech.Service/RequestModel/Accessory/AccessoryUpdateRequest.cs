﻿namespace TerrariumGardenTech.Service.RequestModel.Accessory;

public class AccessoryUpdateRequest
{
    public int AccessoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}