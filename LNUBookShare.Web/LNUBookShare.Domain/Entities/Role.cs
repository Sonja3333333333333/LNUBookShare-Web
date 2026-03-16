using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LNUBookShare.Domain.Entities;

public partial class Role : IdentityRole<int>
{
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
