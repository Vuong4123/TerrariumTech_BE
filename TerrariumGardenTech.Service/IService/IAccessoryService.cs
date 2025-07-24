using TerrariumGardenTech.Common.RequestModel.Accessory;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IAccessoryService
{
    Task<IBusinessResult> GetAll(AccessoryGetAllRequest request);
    Task<IBusinessResult> GetByAccesname(string name);
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> FilterAccessoryAsync(int? categoryId);
    Task<IBusinessResult> CreateAccessory(AccessoryCreateRequest accessoryCreateRequest);
    Task<IBusinessResult> UpdateAccessory(AccessoryUpdateRequest accessoryUpdateRequest);
    Task<IBusinessResult> Save(Accessory accessory);
    Task<IBusinessResult> DeleteById(int id);
}