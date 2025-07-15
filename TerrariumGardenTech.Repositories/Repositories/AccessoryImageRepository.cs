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
    public class AccessoryImageRepository : GenericRepository<AccessoryImage>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public AccessoryImageRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        
    }
}
