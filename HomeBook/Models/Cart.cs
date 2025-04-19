using System;
using System.Collections.Generic;

namespace HomeBook.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? CustomerId { get; set; }

    public int? BookId { get; set; }

    public int Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Customer? Customer { get; set; }
}
