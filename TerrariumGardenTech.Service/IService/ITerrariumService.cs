using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumService
{
    Task<IBusinessResult> GetAll(TerrariumGetAllRequest request);

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
}