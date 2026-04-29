using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class AdminReportService : IAdminReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<AdminReportService> _logger;

        public AdminReportService(IReportRepository reportRepository, ILogger<AdminReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<UserReport>>> GetAllReportsAsync()
        {
            _logger.LogInformation("Адміністратор запитує список усіх скарг.");

            var reports = await _reportRepository.GetAllWithUsersAsync();

            if (reports == null)
            {
                _logger.LogWarning("Список скарг повернув null.");
                return Result<IEnumerable<UserReport>>.Failure("Дані про скарги відсутні.");
            }

            _logger.LogInformation("Успішно отримано {Count} скарг.", reports.Count());
            return Result<IEnumerable<UserReport>>.Success(reports);
        }

        public async Task<Result<IEnumerable<UserReport>>> GetReportsAsync(string? query, string searchBy = "sender", string sortBy = "date", string statusFilter = "active", string? reasonFilter = null)
        {
            _logger.LogInformation("Запит на отримання скарг: SearchBy={SearchBy}, Status={Status}, Reason={Reason}, Query='{Query}'", searchBy, statusFilter, reasonFilter, query);

            var trimmedQuery = string.IsNullOrWhiteSpace(query) ? string.Empty : query.Trim();

            var reports = await _reportRepository.GetFilteredReportsAsync(searchBy, trimmedQuery, sortBy, statusFilter, reasonFilter);

            if (reports == null)
            {
                _logger.LogWarning("Репозиторій повернув null для фільтрованого списку скарг.");
                return Result<IEnumerable<UserReport>>.Failure("Дані про скарги відсутні.");
            }

            _logger.LogInformation("Успішно знайдено {Count} скарг.", reports.Count());
            return Result<IEnumerable<UserReport>>.Success(reports);
        }

        public async Task<Result> ResolveReportAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("Спробу вирішення скарги відхилено: ID {ReportId} не знайдено", reportId);
                return Result.Failure("Скаргу не знайдено у базі даних.");
            }

            if (report.Status == ReportStatus.Resolved)
            {
                return Result.Failure("Ця скарга вже має статус 'Вирішена'.");
            }

            await _reportRepository.UpdateStatusAsync(reportId, ReportStatus.Resolved);

            _logger.LogInformation("Статус скарги {ReportId} успішно змінено на Resolved", reportId);
            return Result.Success();
        }

        public async Task<Result> DeleteReportAsync(int reportId)
        {
            if (reportId <= 0)
            {
                return Result.Failure("Некоректний ID.");
            }

            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                return Result.Failure("Скаргу не знайдено.");
            }

            await _reportRepository.DeleteAsync(reportId);

            _logger.LogInformation("Скаргу {ReportId} видалено з бази даних", reportId);
            return Result.Success();
        }
    }
}
