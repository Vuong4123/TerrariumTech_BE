using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.ResponseModel.Terrarium;

namespace TerrariumGardenTech.Service.Mappers
{
    public static class TerrariumMapper
    {
        public static TerrariumResponse ToTerrariumResponse(this Terrarium terrarium)
        {
            return new TerrariumResponse
            {
                TerrariumId = terrarium.TerrariumId,
                Name = terrarium.TerrariumName,
                Description = terrarium.Description,
                Price = terrarium.Price ?? 0,
                Stock = terrarium.Stock,
                Status = terrarium.Status,
                CreatedAt = terrarium.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = terrarium.UpdatedAt ?? DateTime.MinValue,
                BodyHTML = terrarium.bodyHTML ?? string.Empty,
                Environments = terrarium.TerrariumEnvironments.Select(e => e.EnvironmentTerrarium?.EnvironmentName).ToList(),
                Shapes = terrarium.TerrariumShapes.Select(s => s.Shape?.ShapeName ).ToList(),
                TankMethods = terrarium.TerrariumTankMethods.Select(t => t.TankMethod?.TankMethodType).ToList(),
                Accessories = terrarium.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList() ?? [],
                TerrariumImages = terrarium.TerrariumImages?.Select(i => new TerrariumImageResponse
                {
                    ImageUrl = i.ImageUrl ,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary
                }).ToList() ?? []
            };
        }
    }
}
