using TerrariumGardenTech.Common.RequestModel.Personalize;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IPersonalizeService
{
    Task<IBusinessResult> GetAllPersonalize();
    Task<IBusinessResult> GetPersonalizeById(int id);
    Task<IBusinessResult> CreatePersonalize(PersonalizeCreateRequest personalizeCreateRequest);
    Task<IBusinessResult> UpdatePersonalize(PersonalizeUpdateRequest personalizeUpdateRequest);
    Task<IBusinessResult> SavePersonalize(Personalize personalize);
    Task<IBusinessResult> DeletePersonalizeById(int id);
}