using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IFacultyService
    {
        Task<Result<IEnumerable<Faculty>>> GetAllFacultiesAsync();
    }
}