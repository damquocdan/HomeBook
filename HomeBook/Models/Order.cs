using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Order
{
    [DisplayName("Mã đơn hàng")]
    public int OrderId { get; set; }

    [DisplayName("Khách hàng")]
    public int? CustomerId { get; set; }

    [DisplayName("Ngày đặt hàng")]
    [DataType(DataType.DateTime)]
    public DateTime? OrderDate { get; set; }

    [DisplayName("Tổng tiền")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }

    [DisplayName("Trạng thái")]
    public string? Status { get; set; }

    [DisplayName("Khách hàng")]
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}