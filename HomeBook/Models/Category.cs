using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.Models;

public partial class Category
{
    [DisplayName("Mã danh mục")]
    public int CategoryId { get; set; }

    [DisplayName("Tên danh mục")]
    [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}