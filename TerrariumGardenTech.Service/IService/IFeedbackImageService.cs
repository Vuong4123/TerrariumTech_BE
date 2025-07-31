using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Common.RequestModel.FeedbackImage;

namespace FeedbackGardenTech.Service.IService
{
    public interface IFeedbackImageService
    {
        Task<IBusinessResult> CreateFeedbackImageAsync(IFormFile imageFile, int feedbackId);
        Task<IBusinessResult> UpdateFeedbackImageAsync(FeedbackImageUploadUpdateRequest request);
        Task<IBusinessResult> DeleteFeedbackImageAsync(int feedbackId);
        Task<IBusinessResult?> GetFeedbackImageByIdAsync(int feedbackId);
        Task<IBusinessResult> GetByFeedbackId(int feedbackId);
        Task<IBusinessResult> GetAllFeedbackImageAsync();
    }
}
