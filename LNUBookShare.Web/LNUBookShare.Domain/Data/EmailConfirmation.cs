using System;
using System.Collections.Generic;

namespace LNUBookShare.Infrastructure.Data;

public partial class EmailConfirmation
{
    public int ConfirmationId { get; set; }

    public int UserId { get; set; }

    public string ConfirmationToken { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual User User { get; set; } = null!;
}
