using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.ResponseModel.Feedback;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>
    {
        public FeedbackRepository(TerrariumGardenTechDBContext ctx) : base(ctx) { }
        private IQueryable<Feedback> BaseQuery()
        {
            return _context.Feedbacks
                .Include(f => f.FeedbackImages)
                .Include(f => f.OrderItem)
                    .ThenInclude(oi => oi.TerrariumVariant)
                        .ThenInclude(tv => tv.Terrarium) // để lấy TerrariumId/Name nếu cần
                .Include(f => f.OrderItem)
                    .ThenInclude(oi => oi.Accessory);
        }
        // Lấy feedback theo OrderItemId
        public async Task<List<Feedback>> GetByOrderItemAsync(int orderItemId)
        {
            return await BaseQuery()
                .Where(f => f.OrderItemId == orderItemId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        // Lấy feedback theo Id
        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(f => f.FeedbackId == id && !f.IsDeleted);
        }

        // Trung bình rating theo user
        public async Task<double?> GetAverageRatingByUserAsync(int userId)
        {
            return await _context.Feedbacks
                .Where(f => f.UserId == userId && f.Rating.HasValue && !f.IsDeleted)
                .AverageAsync(f => (double?)f.Rating);
        }

        // 1 user chỉ được feedback 1 lần cho mỗi orderItem (bỏ qua bản đã xoá mềm)
        public async Task<bool> ExistsByOrderItemAndUserAsync(int orderItemId, int userId)
        {
            return await _context.Feedbacks
                .AnyAsync(f => f.OrderItemId == orderItemId
                               && f.UserId == userId
                               && !f.IsDeleted);
        }

        // Get all có paging
        public async Task<(List<FeedbackResponse> Items, int Total)> GetAllDtoAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var q = _context.Feedbacks.Where(f => !f.IsDeleted);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackResponse
                {
                    FeedbackId = f.FeedbackId,
                    OrderItemId = f.OrderItemId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,

                    // Terrarium gốc qua Variant (an toàn null)
                    TerrariumId = f.OrderItem.TerrariumVariant != null
                                    ? (int?)f.OrderItem.TerrariumVariant.TerrariumId
                                    : null,
                    TerrariumName = f.OrderItem.TerrariumVariant != null
                                    ? f.OrderItem.TerrariumVariant.Terrarium.TerrariumName // hoặc .Name nếu Terrarium có Name
                                    : null,

                    // Accessory
                    AccessoryId = f.OrderItem.AccessoryId,
                    AccessoryName = f.OrderItem.Accessory != null
                                    ? f.OrderItem.Accessory.Name                   // hoặc .Title nếu field là Title
                                    : null,

                    // Ảnh
                    Images = f.FeedbackImages
                              .Select(img => new FeedbackImageResponse
                              {
                                  FeedbackId = img.FeedbackId,
                                  FeedbackImageId = img.FeedbackImageId,
                                  Url = img.ImageUrl
                              })
                              .ToList()
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<(List<FeedbackResponse> Items, int Total)>
    GetAllByUserDtoAsync(int userId, int page, int pageSize)
        {
            var q = _context.Feedbacks.Where(f => !f.IsDeleted && f.UserId == userId);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(f => new FeedbackResponse
                {
                    FeedbackId = f.FeedbackId,
                    OrderItemId = f.OrderItemId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    TerrariumId = f.OrderItem.TerrariumVariant != null
                                    ? (int?)f.OrderItem.TerrariumVariant.TerrariumId : null,
                    TerrariumName = f.OrderItem.TerrariumVariant != null
                                    ? f.OrderItem.TerrariumVariant.Terrarium.TerrariumName : null,
                    AccessoryId = f.OrderItem.AccessoryId,
                    AccessoryName = f.OrderItem.Accessory != null ? f.OrderItem.Accessory.Name : null,
                    Images = f.FeedbackImages.Select(img => new FeedbackImageResponse
                    {
                        FeedbackId = img.FeedbackId,
                        FeedbackImageId = img.FeedbackImageId,
                        Url = img.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<(List<FeedbackResponse> Items, int Total)>
            GetByTerrariumDtoAsync(int terrariumId, int page, int pageSize)
        {
            var q = _context.Feedbacks.Where(f =>
                !f.IsDeleted &&
                f.OrderItem.TerrariumVariant != null &&
                f.OrderItem.TerrariumVariant.TerrariumId == terrariumId);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(f => new FeedbackResponse
                {
                    FeedbackId = f.FeedbackId,
                    OrderItemId = f.OrderItemId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    TerrariumId = f.OrderItem.TerrariumVariant.TerrariumId,
                    TerrariumName = f.OrderItem.TerrariumVariant.Terrarium.TerrariumName, // hoặc .Name
                    AccessoryId = f.OrderItem.AccessoryId,
                    AccessoryName = f.OrderItem.Accessory != null ? f.OrderItem.Accessory.Name : null,
                    Images = f.FeedbackImages.Select(img => new FeedbackImageResponse
                    {
                        FeedbackId = img.FeedbackId,
                        FeedbackImageId = img.FeedbackImageId,
                        Url = img.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<(List<FeedbackResponse> Items, int Total)>
            GetByAccessoryDtoAsync(int accessoryId, int page, int pageSize)
        {
            var q = _context.Feedbacks.Where(f => !f.IsDeleted && f.OrderItem.AccessoryId == accessoryId);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(f => new FeedbackResponse
                {
                    FeedbackId = f.FeedbackId,
                    OrderItemId = f.OrderItemId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    TerrariumId = f.OrderItem.TerrariumVariant != null
                                    ? (int?)f.OrderItem.TerrariumVariant.TerrariumId : null,
                    TerrariumName = f.OrderItem.TerrariumVariant != null
                                    ? f.OrderItem.TerrariumVariant.Terrarium.TerrariumName : null,
                    AccessoryId = f.OrderItem.AccessoryId,
                    AccessoryName = f.OrderItem.Accessory != null ? f.OrderItem.Accessory.Name : null,
                    Images = f.FeedbackImages.Select(img => new FeedbackImageResponse
                    {
                        FeedbackId = img.FeedbackId,
                        FeedbackImageId = img.FeedbackImageId,
                        Url = img.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<FeedbackResponse>> GetByOrderItemAsyncV2(int orderItemId)
        {
            return await _context.Feedbacks
                .Where(f => !f.IsDeleted && f.OrderItemId == orderItemId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackResponse
                {
                    FeedbackId = f.FeedbackId,
                    OrderItemId = f.OrderItemId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    TerrariumId = f.OrderItem.TerrariumVariant != null
                                        ? (int?)f.OrderItem.TerrariumVariant.TerrariumId : null,
                    TerrariumName = f.OrderItem.TerrariumVariant != null
                                        ? f.OrderItem.TerrariumVariant.Terrarium.TerrariumName : null,
                    AccessoryId = f.OrderItem.AccessoryId,
                    AccessoryName = f.OrderItem.Accessory != null
                                        ? f.OrderItem.Accessory.Name : null,
                    Images = f.FeedbackImages.Select(img => new FeedbackImageResponse
                    {
                        FeedbackId = img.FeedbackId,
                        FeedbackImageId = img.FeedbackImageId,
                        Url = img.ImageUrl
                    }).ToList()
                })
                .ToListAsync();
        }

        // Soft‑delete
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var e = await _context.Feedbacks.FindAsync(id);
            if (e == null || e.IsDeleted) return false;
            e.IsDeleted = true;
            e.DeletedAt = DateTime.UtcNow;
            e.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Update entity
        public async Task UpdateAsync(Feedback entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Feedbacks.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

}
