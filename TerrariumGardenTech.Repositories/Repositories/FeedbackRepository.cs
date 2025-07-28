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
        public FeedbackRepository(TerrariumGardenTechDBContext context) : base(context) { }

        public async Task<List<Feedback>> GetByOrderItemAsync(int orderItemId)
            => await _context.Feedbacks
                             .Include(f => f.FeedbackImages)
                             .Where(f => f.OrderItemId == orderItemId)
                             .ToListAsync();

        public async Task<double?> GetAverageRatingByUserAsync(int userId)
            => await _context.Feedbacks
                             .Where(f => f.UserId == userId && f.Rating.HasValue)
                             .AverageAsync(f => (double?)f.Rating);
    }

}
