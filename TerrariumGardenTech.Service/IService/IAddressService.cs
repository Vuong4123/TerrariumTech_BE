using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.Address;

namespace TerrariumGardenTech.Service.IService
{
    public interface IAddressService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateAddress(AddressCreateRequest addressCreateRequest);
        Task<IBusinessResult> UpdateAddress(AddressUpdateRequest addressUpdateRequest);
        Task<IBusinessResult> Save(Address address );
        Task<IBusinessResult> DeleteById(int id);
    }
}
