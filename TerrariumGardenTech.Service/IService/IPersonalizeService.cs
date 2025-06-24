using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Category;
using TerrariumGardenTech.Service.RequestModel.Personalize;

namespace TerrariumGardenTech.Service.IService
{
    public interface IPersonalizeService
    {
        Task<IBusinessResult> GetAllPersonalize();
        Task<IBusinessResult> GetPersonalizeById(int id);
        Task<IBusinessResult> CreatePersonalize(PersonalizeCreateRequest personalizeCreateRequest);
        Task<IBusinessResult> UpdatePersonalize(PersonalizeUpdateRequest personalizeUpdateRequest);
        Task<IBusinessResult> SavePersonalize(Personalize personalize);
        Task<IBusinessResult> DeletePersonalizeById(int id);
    }
}
