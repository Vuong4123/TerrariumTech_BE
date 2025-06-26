using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class AddressRepository : GenericRepository<Address>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public AddressRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    }
}
