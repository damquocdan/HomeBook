using System;
using System.Collections.Generic;

namespace HomeBook.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? CustomerId { get; set; }

    public int? BookId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Customer? Customer { get; set; }
}
