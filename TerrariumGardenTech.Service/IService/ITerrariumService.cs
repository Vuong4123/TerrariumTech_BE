using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumService
{
    Task<IBusinessResult> GetAll(TerrariumGetAllRequest request);

    // Task<BusinessResult> GetAllOfParam(string type, string shape, string tankMethod, string theme, string size);
    Task<IBusinessResult> GetTerrariumSuggestions(int userId);
    Task<IBusinessResult> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId);
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest);
    Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest);
    Task<IBusinessResult> Save(Terrarium terrarium);
    Task<IBusinessResult> DeleteById(int id);
    Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName);
}