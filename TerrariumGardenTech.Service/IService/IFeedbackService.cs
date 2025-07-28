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
        Task<FeedbackResponse> CreateAsync(FeedbackCreateRequest request, int userId);
        Task<List<FeedbackResponse>> GetByOrderItemAsync(int orderItemId);
    }
}
