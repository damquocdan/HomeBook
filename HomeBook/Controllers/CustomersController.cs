using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace HomeBook.Controllers
{
    public class CustomersController : Controller
    {
        private readonly HomeBookContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CustomersController(HomeBookContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Customers/Details
        public async Task<IActionResult> Details()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Index", "LoginC");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == customerId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Edit
        public async Task<IActionResult> Edit()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để chỉnh sửa thông tin cá nhân.";
                return RedirectToAction("Index", "LoginC");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,Username,Password,FullName,Email,Phone,Address,Avatar,CreatedAt")] Customer customer, IFormFile avatarFile)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null || id != customerId || id != customer.CustomerId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin này.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                        return NotFound();
                    }

                    // Chỉ cập nhật các trường cho phép
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;

                    // Xử lý tải lên ảnh avatar
                    if (avatarFile != null && avatarFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads/Avatars");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await avatarFile.CopyToAsync(fileStream);
                        }

                        existingCustomer.Avatar = "/Uploads/Avatars/" + uniqueFileName;
                    }

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật thông tin cá nhân thành công.";
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.";
                    return View(customer);
                }
            }

            TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View(customer);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}