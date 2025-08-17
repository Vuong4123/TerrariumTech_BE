using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface IComboService
    {
        Task<IBusinessResult> GetAllCombosAsync(GetCombosRequest request);

        Task<IBusinessResult> GetFeaturedCombosAsync(int take = 10);

        Task<IBusinessResult> GetCombosByCategoryAsync(int categoryId, int page = 1, int pageSize = 12);

        Task<IBusinessResult> GetComboByIdAsync(int id);

        Task<IBusinessResult> CreateComboAsync(CreateComboRequest request);

        Task<IBusinessResult> UpdateComboAsync(UpdateComboRequest request);

        Task<IBusinessResult> DeleteComboAsync(int id);

        Task<IBusinessResult> ToggleComboActiveAsync(int id);

        Task<IBusinessResult> UpdateComboStockAsync(int comboId, int quantity);
    }
}