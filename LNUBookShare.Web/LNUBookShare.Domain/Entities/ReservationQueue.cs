using System;
using System.Collections.Generic;

namespace LNUBookShare.Domain.Entities;

public partial class ReservationQueue
{
    public int QueueId { get; set; }

    public int BookId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
