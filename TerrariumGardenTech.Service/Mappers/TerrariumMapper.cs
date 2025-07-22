using TerrariumGardenTech.Common.ResponseModel.Terrarium;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.Mappers;

public static class TerrariumMapper
{
    public static TerrariumResponse ToTerrariumResponse(this Terrarium terrarium)
    {
        return new TerrariumResponse
        {
            TerrariumId = terrarium.TerrariumId,
            EnvironmentId = terrarium.EnvironmentId,
            ShapeId = terrarium.ShapeId,
            TankMethodId = terrarium.TankMethodId,
            Name = terrarium.TerrariumName,
            Description = terrarium.Description,
            Price = terrarium.Price ?? 0,
            Stock = terrarium.Stock,
            Status = terrarium.Status,
            CreatedAt = terrarium.CreatedAt ?? DateTime.MinValue,
            UpdatedAt = terrarium.UpdatedAt ?? DateTime.MinValue,
            BodyHTML = terrarium.bodyHTML ?? string.Empty,
            Accessories = terrarium.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
            {
                AccessoryId = a.Accessory.AccessoryId,
                Name = a.Accessory.Name,
                Description = a.Accessory.Description,
                Price = a.Accessory.Price
            }).ToList() ?? [],
            TerrariumImages = terrarium.TerrariumImages?.Select(i => new TerrariumImageResponse
            {
                TerrariumImageId = i.TerrariumImageId,
                TerrariumId = i.TerrariumId,
                ImageUrl = i.ImageUrl
            }).ToList() ?? []
        };
    }
}