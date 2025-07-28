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

        public async Task<FeedbackResponse> CreateAsync(FeedbackCreateRequest request, int userId)
        {
            // 1. Map request → entity (chưa có FK, chưa có ảnh)
            var entity = _mapper.Map<Feedback>(request);

            // 2. Gán FK User
            entity.UserId = userId;

            // 3. Khởi tạo danh sách ảnh (nếu có)
            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                entity.FeedbackImages = request.ImageUrls
                                               .Select(url => new FeedbackImage { ImageUrl = url })
                                               .ToList();
            }

            // 4. Lưu vào DB (CreateAsync trong GenericRepository chỉ Add + SaveChanges)
            await _uow.Feedback.CreateAsync(entity);
            // Nếu CreateAsync đã SaveChanges, không cần _uow.SaveAsync() nữa.

            // 5. Trả về DTO
            return _mapper.Map<FeedbackResponse>(entity);
        }

        public async Task<List<FeedbackResponse>> GetByOrderItemAsync(int orderItemId)
        {
            var list = await _uow.Feedback.GetByOrderItemAsync(orderItemId);
            return _mapper.Map<List<FeedbackResponse>>(list);
        }
    }
}
