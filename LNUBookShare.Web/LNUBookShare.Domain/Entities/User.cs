using System;
using System.Collections.Generic;

namespace LNUBookShare.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int FacultyId { get; set; }

    public int RoleId { get; set; }

    public int? AvatarId { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public string? ApiToken { get; set; }

    public decimal AvgRating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Image? Avatar { get; set; }

    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<ChatMessage> ChatMessageReceivers { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageSenders { get; set; } = new List<ChatMessage>();

    public virtual EmailConfirmation? EmailConfirmation { get; set; }

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<ReservationQueue> ReservationQueues { get; set; } = new List<ReservationQueue>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserReview> UserReviewOwners { get; set; } = new List<UserReview>();

    public virtual ICollection<UserReview> UserReviewReviewers { get; set; } = new List<UserReview>();
}
