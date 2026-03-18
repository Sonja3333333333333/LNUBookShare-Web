using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace LNUBookShare.Domain.Entities;

public partial class Role : IdentityRole<int> // ДОДАНО: Наслідування
{
    // public int RoleId { get; set; } // ЗАМІНЕНО на Id
    // public string RoleName { get; set; } = null!; // ЗАМІНЕНО на Name

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}