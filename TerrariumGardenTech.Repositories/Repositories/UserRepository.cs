using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByUsernameAsync(string username);
        Task CreateUserAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly TerrariumGardenTechDBContext _context;

        public UserRepository(TerrariumGardenTechDBContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
