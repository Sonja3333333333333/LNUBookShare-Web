using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces;

public interface IReportService
{
    Task<Result> CreateReportAsync(int senderId, int reportedId, ReportReason reason, string details);
}