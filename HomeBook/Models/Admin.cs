using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Admin
{
    [DisplayName("Mã Admin")]
    public int AdminId { get; set; }

    [DisplayName("Tên đăng nhập")]
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
    public string Username { get; set; } = null!;

    [DisplayName("Mật khẩu")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DisplayName("Họ và tên")]
    public string? FullName { get; set; }

    [DisplayName("Email")]
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [DisplayName("Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Phone { get; set; }
}