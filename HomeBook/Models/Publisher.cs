using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Publisher
{
    [DisplayName("Mã nhà xuất bản")]
    public int PublisherId { get; set; }

    [DisplayName("Tên nhà xuất bản")]
    [Required(ErrorMessage = "Tên nhà xuất bản là bắt buộc.")]
    public string PublisherName { get; set; } = null!;

    [DisplayName("Người liên hệ")]
    public string? ContactPerson { get; set; }

    [DisplayName("Email")]
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [DisplayName("Điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Phone { get; set; }

    [DisplayName("Địa chỉ")]
    public string? Address { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}