using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class AdminTransactionService : IAdminTransactionService
    {
        private readonly IRentalTransactionRepository _transactionRepository;
        private readonly ILogger<AdminTransactionService> _logger;

        public AdminTransactionService(
            IRentalTransactionRepository transactionRepository,
            ILogger<AdminTransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<RentalTransaction>>> GetTransactionsAsync(string? searchBy, string? searchQuery, string? sortBy, string? statusFilter, string? termFilter)
        {
            _logger.LogInformation("Адміністратор запитує список транзакцій. Пошук: {SearchQuery}", searchQuery);

            try
            {
                var transactions = await _transactionRepository.GetAllWithDetailsAsync(searchBy, searchQuery, sortBy, statusFilter, termFilter);

                return Result<IEnumerable<RentalTransaction>>.Success(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні транзакцій для адміністратора.");
                return Result<IEnumerable<RentalTransaction>>.Failure("Не вдалося завантажити список транзакцій.");
            }
        }
    }
}
