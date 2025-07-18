using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Environment;

namespace TerrariumGardenTech.Service.IService;

public interface IEnvironmentService
{
    Task<IBusinessResult> CreateEnvironmentAsync(EnvironmentCreateRequest environmentCreateRequest);
    Task<IBusinessResult> UpdateEnvironmentAsync(EnvironmentUpdateRequest environmentUpdateRequest);
    Task<IBusinessResult> DeleteEnvironmentAsync(int environmentId);
    Task<IBusinessResult?> GetEnvironmentByIdAsync(int environmentId);
    Task<IBusinessResult> GetAllEnvironmentsAsync();
}