using LNUBookShare.Application.Common;

public interface IReportService
{
    Task<Result> CreateReportAsync(int senderId, int reportedId, string context);
}