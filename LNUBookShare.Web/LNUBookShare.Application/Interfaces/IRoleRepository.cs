using LNUBookShare.Domain.Entities;


namespace LNUBookShare.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task AddAsync(Role role);

        Task ClearAllAsync();
    }

}
