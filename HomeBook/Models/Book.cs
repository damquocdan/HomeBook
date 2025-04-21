using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Book
{
    [DisplayName("Mã sách")]
    public int BookId { get; set; }

    [DisplayName("Tên sách")]
    [Required(ErrorMessage = "Tên sách là bắt buộc.")]
    public string BookTitle { get; set; } = null!;

    [DisplayName("Tác giả")]
    [Required(ErrorMessage = "Tác giả là bắt buộc.")]
    public string Author { get; set; } = null!;

    [DisplayName("Mô tả")]
    public string? Description { get; set; }

    [DisplayName("Giá")]
    [Required(ErrorMessage = "Giá là bắt buộc.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [DisplayName("Số lượng tồn kho")]
    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
    public int StockQuantity { get; set; }

    [DisplayName("Số lượng đã đặt")]
    public int? OrderedQuantity { get; set; }

    [DisplayName("Danh mục")]
    public int? CategoryId { get; set; }

    [DisplayName("Nhà xuất bản")]
    public int? PublisherId { get; set; }

    [DisplayName("Trạng thái")]
    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    public string Status { get; set; } = null!;

    [DisplayName("Ảnh bìa")]
    public string? CoverImageUrl { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [DisplayName("Danh mục")]
    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [DisplayName("Nhà xuất bản")]
    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}