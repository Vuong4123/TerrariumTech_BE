using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface IProfileService
    {
        Task<IBusinessResult> GetMyProfileAsync(int userId);
        Task<IBusinessResult> EditMyProfileAsync(int userId, EditUserProfileRequest req);
        Task<IBusinessResult> UploadAvatarAsync(int userId, IFormFile file);
        Task<IBusinessResult> UploadBackgroundAsync(int userId, IFormFile file);

        // Admin
        Task<IBusinessResult> AdminGetAllProfilesAsync();
        Task<IBusinessResult> AdminGetProfileByUserIdAsync(int userId);
    }
}
