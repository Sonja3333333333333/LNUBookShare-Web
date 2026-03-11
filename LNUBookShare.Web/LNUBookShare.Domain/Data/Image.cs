using System;
using System.Collections.Generic;

namespace LNUBookShare.Infrastructure.Data;

public partial class Image
{
    public int ImageId { get; set; }

    public string ImagePath { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public string ImageType { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
