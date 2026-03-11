using System;
using System.Collections.Generic;

namespace LNUBookShare.Domain.Entities;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string? Isbn { get; set; }

    public int Year { get; set; }

    public string? Publisher { get; set; }

    public string? Language { get; set; }

    public int CategoryId { get; set; }

    public int OwnerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CoverId { get; set; }

    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();

    public virtual Category Category { get; set; } = null!;

    public virtual Image? Cover { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<ReservationQueue> ReservationQueues { get; set; } = new List<ReservationQueue>();
}
