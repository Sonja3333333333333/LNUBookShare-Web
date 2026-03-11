using System;
using System.Collections.Generic;

namespace LNUBookShare.Infrastructure.Data;

public partial class BookReview
{
    public int ReviewId { get; set; }

    public int BookId { get; set; }

    public int ReviewerId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
