using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserDetailsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Faculty)
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}