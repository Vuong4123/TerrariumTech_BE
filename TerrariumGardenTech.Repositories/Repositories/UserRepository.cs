using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{

    public class UserRepository : GenericRepository<User>
    {
        private readonly TerrariumGardenTechDBContext _context;

        public UserRepository(TerrariumGardenTechDBContext context)
        {
            _context = context;
        }

       
    }
}
