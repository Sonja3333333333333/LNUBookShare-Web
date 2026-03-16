// <copyright file="UserReview.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LNUBookShare.Domain.Entities;

public partial class UserReview
{
    public int ReviewId { get; set; }

    public int OwnerId { get; set; }

    public int ReviewerId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
