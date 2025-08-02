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
    public class FeedbackImageRepository : GenericRepository<FeedbackImage>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public FeedbackImageRepository(TerrariumGardenTechDBContext dbContext) 
            => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<IEnumerable<FeedbackImage>> GetAllByFeedbackIdAsync(int feedbackId)
        {
            return await _context.FeedbackImages
                .Where(ti => ti.FeedbackId == feedbackId)
                .ToListAsync();
        }
    }

}
