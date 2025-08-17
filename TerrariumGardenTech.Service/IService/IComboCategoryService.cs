using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IComboCategoryService
{
    Task<IBusinessResult> GetAllCategoriesAsync(bool includeInactive = false);

    Task<IBusinessResult> GetCategoryByIdAsync(int id);

    Task<IBusinessResult> CreateCategoryAsync(CreateComboCategoryRequest request);

    Task<IBusinessResult> UpdateCategoryAsync(UpdateComboCategoryRequest request);

    Task<IBusinessResult> DeleteCategoryAsync(int id);
}