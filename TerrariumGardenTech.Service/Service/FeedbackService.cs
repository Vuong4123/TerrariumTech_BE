using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Common.ResponseModel.Feedback;

namespace TerrariumGardenTech.Service.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly UnitOfWork _uow;
        private readonly IMapper _mapper;

        public FeedbackService(UnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<FeedbackResponse> CreateAsync(FeedbackCreateRequest request)
        {
            var entity = _mapper.Map<Feedback>(request);
            await _uow.Feedback.CreateAsync(entity);
            await _uow.SaveAsync();

            return _mapper.Map<FeedbackResponse>(entity);
        }

        public async Task<List<FeedbackResponse>> GetByOrderItemAsync(int orderItemId)
        {
            var list = await _uow.Feedback.GetByOrderItemAsync(orderItemId);
            return _mapper.Map<List<FeedbackResponse>>(list);
        }
    }
}
