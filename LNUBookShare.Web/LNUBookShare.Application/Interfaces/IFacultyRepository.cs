using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces;

public interface IFacultyRepository
{
    Task<IEnumerable<Faculty>> GetAllAsync(); // ДОДАЙ ЦЕ
    Task AddAsync(Faculty faculty);
    Task ClearAllAsync();
}