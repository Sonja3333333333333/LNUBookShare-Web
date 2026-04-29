using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminReportService
    {
        Task<Result<IEnumerable<UserReport>>> GetAllReportsAsync();
        Task<Result<IEnumerable<UserReport>>> GetReportsAsync(string query, string searchBy = "sender", string sortBy = "date", string statusFilter = "active");
        Task<Result> ResolveReportAsync(int reportId);
        Task<Result> DeleteReportAsync(int reportId);
    }
}
