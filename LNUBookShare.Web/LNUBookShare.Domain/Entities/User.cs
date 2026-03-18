// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;

namespace LNUBookShare.Domain.Entities;

public partial class User : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int FacultyId { get; set; }
    public int RoleId { get; set; } = 2;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Навігаційні властивості
    public virtual Faculty Faculty { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}