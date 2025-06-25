using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Category;

namespace TerrariumGardenTech.Service.IService
{
    public interface ICategoryService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateCategory(CategoryCreateRequest categoryRequest);
        Task<IBusinessResult> UpdateCategory(CategoryUpdateRequest categoryRequest);
        Task<IBusinessResult> Save(Category category);
        Task<IBusinessResult> DeleteById(int id);
    }
}
