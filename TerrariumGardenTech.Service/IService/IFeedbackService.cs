using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Common.ResponseModel.Feedback;

namespace TerrariumGardenTech.Service.IService
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> CreateAsync(FeedbackCreateRequest req, int userId);
        Task<(IEnumerable<FeedbackResponse> Items, int Total)> GetAllAsync(int page, int pageSize);
        Task<List<FeedbackResponse>> GetByOrderItemAsync(int orderItemId);
        Task<FeedbackResponse> UpdateAsync(int id, FeedbackUpdateRequest req, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
