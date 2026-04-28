using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<Result> CreateReportAsync(int senderId, int reportedId, string context)
        {
            if (string.IsNullOrWhiteSpace(context))
            {
                return Result.Failure("Опишіть причину скарги.");
            }

            if (senderId == reportedId)
            {
                return Result.Failure("Не можна скаржитись на самого себе.");
            }

            if (await _reportRepository.ExistsAsync(senderId, reportedId))
            {
                return Result.Failure("Ви вже надсилали скаргу на цього користувача.");
            }

            var report = new Report
            {
                SenderId = senderId,
                ReportedUserId = reportedId,
                Context = context,
                CreatedAt = DateTime.UtcNow,
            };

            await _reportRepository.AddAsync(report);
            return Result.Success();
        }
    }
}