using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Services
{
    public class FacultyService : IFacultyService
    {
        private readonly IFacultyRepository _facultyRepository;

        public FacultyService(IFacultyRepository facultyRepository)
        {
            _facultyRepository = facultyRepository;
        }

        public async Task<Result<IEnumerable<Faculty>>> GetAllFacultiesAsync()
        {
            var faculties = await _facultyRepository.GetAllAsync();

            return Result<IEnumerable<Faculty>>.Success(faculties);
        }
    }
}