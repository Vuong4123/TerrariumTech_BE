using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>
    {
        public FeedbackRepository(TerrariumGardenTechDBContext ctx) : base(ctx) { }

        // Giữ nguyên
        public async Task<List<Feedback>> GetByOrderItemAsync(int orderItemId)
            => await _context.Feedbacks
                             .Include(f => f.FeedbackImages)
                             .Where(f => f.OrderItemId == orderItemId)
                             .ToListAsync();

        public async Task<double?> GetAverageRatingByUserAsync(int userId)
            => await _context.Feedbacks
                             .Where(f => f.UserId == userId && f.Rating.HasValue)
                             .AverageAsync(f => (double?)f.Rating);

        // Lấy theo ID (bao gồm ảnh)
        public async Task<Feedback?> GetByIdAsync(int id)
            => await _context.Feedbacks
                             .Include(f => f.FeedbackImages)
                             .FirstOrDefaultAsync(f => f.FeedbackId == id);

        // Lấy theo ID (không bao gồm ảnh)
        public async Task<(List<Feedback> Items, int Total)> GetByTerrariumAsync(int terrariumId, int page, int pageSize)
        {
            var q = _context.Feedbacks
                .Include(f => f.FeedbackImages)
                .Include(f => f.OrderItem)
                    .ThenInclude(oi => oi.TerrariumVariant) // để lấy tên nếu cần
                .Where(f => !f.IsDeleted && f.OrderItem.TerrariumVariantId == terrariumId);

            var total = await q.CountAsync();

            var list = await q.OrderByDescending(f => f.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();

            return (list, total);
        }


        // Get all có paging
        public async Task<(List<Feedback> Items, int Total)> GetAllAsync(int page, int pageSize)
        {
            var q = _context.Feedbacks.Include(f => f.FeedbackImages);
            var total = await q.CountAsync();
            var list = await q.OrderByDescending(f => f.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
            return (list, total);
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
