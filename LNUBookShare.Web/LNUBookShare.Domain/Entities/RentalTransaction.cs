using System;
using System.ComponentModel.DataAnnotations;

namespace LNUBookShare.Domain.Entities
{
    public class RentalTransaction
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public int BorrowerId { get; set; }
        public User Borrower { get; set; } = null!;

        public DateTime IssueDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        public string Status { get; set; } = TransactionStatuses.Active;
    }
}