using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class FacultyRepository : IFacultyRepository
    {

        private readonly AppDbContext _context;
        public FacultyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Faculty faculty)
        {
            await _context.Faculties.AddAsync(faculty);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAllAsync()
        {
            await _context.Faculties.ExecuteDeleteAsync();
        }
    }
}
