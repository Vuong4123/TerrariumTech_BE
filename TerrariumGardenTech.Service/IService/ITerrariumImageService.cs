using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Environment;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITerrariumImageService
    {
        Task<IBusinessResult> CreateTerrariumImageAsync(EnvironmentCreateRequest environmentCreateRequest);
        Task<IBusinessResult> UpdateTerrariumImageAsync(EnvironmentUpdateRequest environmentUpdateRequest);
        Task<IBusinessResult> DeleteTerrariumImageAsync(int environmentId);
        Task<IBusinessResult?> GetTerrariumImageByIdAsync(int environmentId);
        Task<IBusinessResult> GetAllTerrariumImageAsync();
    }
}
