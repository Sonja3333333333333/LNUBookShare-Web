using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IFacultyRepository
    {
        Task AddAsync(Faculty faculty);

        Task ClearAllAsync();
    }
}
