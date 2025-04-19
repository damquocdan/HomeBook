using System;
using System.Collections.Generic;

namespace HomeBook.Models;

public partial class Statistic
{
    public int StatisticId { get; set; }

    public DateOnly ReportDate { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }
}
