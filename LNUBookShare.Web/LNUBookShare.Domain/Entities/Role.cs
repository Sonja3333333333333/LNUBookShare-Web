// <copyright file="Role.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LNUBookShare.Domain.Entities;

public partial class Role : IdentityRole<int>
{
    // public int RoleId { get; set; } // ЗАМІНЕНО на Id
    // public string RoleName { get; set; } = null!; // ЗАМІНЕНО на Name
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}