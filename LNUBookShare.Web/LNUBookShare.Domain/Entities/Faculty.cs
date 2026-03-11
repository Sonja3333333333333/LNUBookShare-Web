using System;
using System.Collections.Generic;

namespace LNUBookShare.Domain.Entities;

public partial class Faculty
{
    public int FacultyId { get; set; }

    public string FacultyName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
