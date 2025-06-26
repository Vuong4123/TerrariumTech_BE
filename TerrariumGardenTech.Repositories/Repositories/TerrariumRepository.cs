using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumRepository : GenericRepository<Terrarium>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        //public TerrariumRepository() { }
        public TerrariumRepository(TerrariumGardenTechDBContext dbContext) =>  _dbContext = dbContext;
        

    }
}
