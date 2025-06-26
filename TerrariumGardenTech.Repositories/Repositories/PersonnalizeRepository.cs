using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class PersonnalizeRepository : GenericRepository<Personalize>
    {
        private readonly TerrariumGardenTechDBContext _dbContexxt;

        public PersonnalizeRepository (TerrariumGardenTechDBContext dbContext) => _dbContexxt = dbContext;
    }
}
