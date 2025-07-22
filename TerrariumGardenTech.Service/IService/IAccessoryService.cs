using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;

namespace TerrariumGardenTech.Service.IService;

public interface IAccessoryService
{
    Task<IBusinessResult> GetAll();
    Task<IBusinessResult> GetAllDetail();
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> FilterAccessoryAsync(int? categoryId);
    Task<IBusinessResult> CreateAccessory(AccessoryCreateRequest accessoryCreateRequest);
    Task<IBusinessResult> UpdateAccessory(AccessoryUpdateRequest accessoryUpdateRequest);
    Task<IBusinessResult> Save(Accessory accessory);
    Task<IBusinessResult> DeleteById(int id);
}