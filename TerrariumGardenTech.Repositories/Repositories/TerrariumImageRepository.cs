using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumImageRepository
    {
        public TerrariumGardenTechDBContext _dbContext;
        public TerrariumImageRepository() { }

        public TerrariumImageRepository(TerrariumGardenTechDBContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
