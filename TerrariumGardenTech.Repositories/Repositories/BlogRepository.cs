using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class BlogRepository : GenericRepository<Blog>
    {
        private readonly TerrariumGardenTechDBContext _context;
        public BlogRepository()
        {
        }
        public BlogRepository(TerrariumGardenTechDBContext context) => _context = context;
    }
}
