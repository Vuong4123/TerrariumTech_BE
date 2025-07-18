using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Address;

namespace TerrariumGardenTech.Service.IService;

public interface IAddressService
{
    // Define methods for address service here, e.g.:
    Task<IBusinessResult> GetAllAddresses();
    Task<IBusinessResult> GetAddressById(int id);
    Task<IBusinessResult> CreateAddress(AddressCreateRequest addressCreateRequest);
    Task<IBusinessResult> UpdateAddress(AddressUpdateRequest addressUpdateRequest);
    Task<IBusinessResult> DeleteAddressById(int id);
}