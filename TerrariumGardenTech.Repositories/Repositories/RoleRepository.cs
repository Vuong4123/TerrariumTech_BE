using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class RoleRepository : GenericRepository<Role>
    {
        public TerrariumGardenTechDBContext _dbContext;
        //public RoleRepository() { }
        public RoleRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        
    }
}
