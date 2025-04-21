using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Contact
{
    [DisplayName("Mã liên hệ")]
    public int ContactId { get; set; }

    [DisplayName("Tên")]
    [Required(ErrorMessage = "Tên là bắt buộc.")]
    public string Name { get; set; } = null!;

    [DisplayName("Email")]
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [DisplayName("Điện thoại")]
    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string Phone { get; set; } = null!;

    [DisplayName("Tiêu đề")]
    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    public string Subject { get; set; } = null!;

    [DisplayName("Nội dung")]
    [Required(ErrorMessage = "Nội dung là bắt buộc.")]
    public string Message { get; set; } = null!;

    [DisplayName("Ngày gửi")]
    [DataType(DataType.DateTime)]
    public DateTime? CreatedAt { get; set; }
}