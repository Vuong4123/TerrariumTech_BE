using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.TerraniumLayout;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common.ResponseModel.TerrariumLayout;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumService
{
    Task<IBusinessResult> GetAll(TerrariumGetAllRequest request);
    Task<IBusinessResult> GetAllGeneratedByAI(TerrariumGetAllRequest request);

    // Task<BusinessResult> GetAllOfParam(string type, string shape, string tankMethod, string theme, string size);
    Task<IBusinessResult> GetTerrariumSuggestions(int userId);
    //Task<IBusinessResult> GetByAccesname(string name);
    Task<IBusinessResult> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId);
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateTerrariumAI(TerrariumCreateRequest terrariumCreateRequest);
    Task<IBusinessResult> CreateTerrarium(TerrariumCreate terrariumCreateRequest);
    Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest);
    Task<IBusinessResult> Save(Terrarium terrarium);
    Task<IBusinessResult> DeleteById(int id);
    Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName);
    Task<AITerrariumResponse> PredictTerrariumAsync(AITerrariumRequest request);

    Task<IBusinessResult> GetTopBestSellersAllTimeAsync(int topN);
    Task<IBusinessResult> GetTopBestSellersLastDaysAsync(int days, int topN);
    Task<IBusinessResult> GetTopRatedAsync(int topN);
    Task<IBusinessResult> GetNewestAsync(int topN);

    Task<TerrariumLayout> CreateAsync(CreateLayoutRequest request);
    Task<bool> UpdateAsync(int id, UpdateLayoutRequest request);
    Task<TerrariumLayoutDetailDto?> GetByIdAsync(int id);
    Task<List<TerrariumLayoutDto>> GetAllAsync();
    Task<List<TerrariumLayoutDto>> GetByUserIdAsync(int userId);
    Task<List<TerrariumLayoutDto>> GetPendingAsync();
    Task<TerrariumLayout> ReviewAsync(int id, int managerId, string status, decimal? price, string? notes);
    Task<IBusinessResult> SubmitLayoutAsync(int layoutId, int userId);
}