using CloudinaryDotNet;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class RentalTransactionRepository : IRentalTransactionRepository
    {
        private readonly AppDbContext _context;

        public RentalTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RentalTransaction transaction)
        {
            await _context.RentalTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<RentalTransaction?> GetActiveByBookIdAsync(int bookId)
        {
            return await _context.RentalTransactions
                .FirstOrDefaultAsync(t => t.BookId == bookId && t.Status == TransactionStatuses.Active);
        }

        public async Task UpdateAsync(RentalTransaction transaction)
        {
            _context.RentalTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RentalTransaction>> GetAllWithDetailsAsync(string? searchBy, string? searchQuery, string? sortBy, string? statusFilter)
        {
            var query = _context.RentalTransactions
                .Include(t => t.Book)
                .Include(t => t.Owner)
                .Include(t => t.Borrower)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                query = query.Where(t => t.Status == statusFilter);
            }
            else
            {
                query = query.Where(t => t.Status == TransactionStatuses.Active || t.Status == TransactionStatuses.Overdue);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();

                query = searchBy switch
                {
                    "owner" => query.Where(t => t.Owner.Email!.ToLower().Contains(searchLower) ||
                                                t.Owner.LastName.ToLower().Contains(searchLower) ||
                                                t.Owner.FirstName.ToLower().Contains(searchLower)),

                    "borrower" => query.Where(t => t.Borrower.Email!.ToLower().Contains(searchLower) ||
                                                   t.Borrower.LastName.ToLower().Contains(searchLower) ||
                                                   t.Borrower.FirstName.ToLower().Contains(searchLower)),

                    _ => query.Where(t => t.Book.Title.ToLower().Contains(searchLower)) // За замовчуванням - книга
                };
            }

            query = sortBy switch
            {
                "date_desc" => query.OrderByDescending(t => t.ExpectedReturnDate),
                "created" => query.OrderByDescending(t => t.IssueDate),
                _ => query.OrderBy(t => t.ExpectedReturnDate)
            };

            return await query.ToListAsync();
        }
    }
}