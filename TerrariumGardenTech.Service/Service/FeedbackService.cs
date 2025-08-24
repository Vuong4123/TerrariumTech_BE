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
            => (_uow, _mapper) = (uow, mapper);

        public async Task<FeedbackResponse> CreateAsync(FeedbackCreateRequest req, int userId)
        {
            var entity = _mapper.Map<Feedback>(req);
            entity.UserId = userId;

            await _uow.Feedback.CreateAsync(entity);

            // Load lại có kèm FeedbackImages để map ra Response.Images
            var created = await _uow.Feedback.GetByIdAsync(entity.FeedbackId);
            return _mapper.Map<FeedbackResponse>(created ?? entity);
        }

        public async Task<(IEnumerable<FeedbackResponse> Items, int Total)> GetAllAsync(int page, int pageSize)
        {
            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _uow.Feedback.GetAllDtoAsync(page, pageSize);
            return (items, total);
        }

        public async Task<(IEnumerable<FeedbackResponse> Items, int Total)> GetAllByUserAsync(int userId, int page, int pageSize)
        {
            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _uow.Feedback.GetAllByUserDtoAsync(userId, page, pageSize);
            return (items, total);
        }

        public async Task<(IEnumerable<FeedbackResponse> Items, int Total)> GetByTerrariumAsync(int terrariumId, int page, int pageSize)
        {
            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _uow.Feedback.GetByTerrariumDtoAsync(terrariumId, page, pageSize);
            return (items, total);
        }

        public async Task<(IEnumerable<FeedbackResponse> Items, int Total)> GetByAccessoryAsync(int accessoryId, int page, int pageSize)
        {
            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _uow.Feedback.GetByAccessoryDtoAsync(accessoryId, page, pageSize);
            return (items, total);
        }

        public async Task<List<FeedbackResponse>> GetByOrderItemAsync(int orderItemId)
        {
            var list = await _uow.Feedback.GetByOrderItemAsync(orderItemId);
            return _mapper.Map<List<FeedbackResponse>>(list);
        }

        public async Task<FeedbackResponse> UpdateAsync(int id, FeedbackUpdateRequest req, int userId)
        {
            var entity = await _uow.Feedback.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Feedback không tồn tại");

            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            _mapper.Map(req, entity); // Profile đã set UpdatedAt
            await _uow.Feedback.UpdateAsync(entity);

            var updated = await _uow.Feedback.GetByIdAsync(id);
            return _mapper.Map<FeedbackResponse>(updated ?? entity);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var e = await _uow.Feedback.GetByIdAsync(id);
            if (e == null || e.IsDeleted) return false;
            if (e.UserId != userId) throw new UnauthorizedAccessException();
            return await _uow.Feedback.SoftDeleteAsync(id);
        }

        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200; // giới hạn tối đa để bảo vệ DB
        }
    }
}
