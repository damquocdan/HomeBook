using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBook.Models;

namespace HomeBook.Controllers
{
    public class BooksController : Controller
    {
        private readonly HomeBookContext _context;

        public BooksController(HomeBookContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(
            string searchString, // Tìm kiếm theo tên sách
            string publisherSearch, // Tìm kiếm theo nhà xuất bản
            int? categoryId, // Lọc theo danh mục
            decimal? minPrice, // Giá tối thiểu
            decimal? maxPrice, // Giá tối đa
            string sortOrder = "newest" // Mặc định sắp xếp theo sách mới nhất
        )
        {
            // Lấy danh sách danh mục và nhà xuất bản để hiển thị trong bộ lọc
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            ViewData["Publishers"] = await _context.Publishers.ToListAsync();

            // Lưu các giá trị lọc để hiển thị lại trên giao diện
            ViewData["CurrentSearchString"] = searchString;
            ViewData["CurrentPublisherSearch"] = publisherSearch;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;
            ViewData["CurrentSortOrder"] = sortOrder;

            // Truy vấn cơ bản
            var books = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .AsQueryable();

            // Tìm kiếm theo tên sách
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.BookTitle.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(publisherSearch))
            {
                books = books.Where(b => b.Publisher.PublisherName.ToLower().Contains(publisherSearch.ToLower()));
            }


            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.Price <= maxPrice.Value);
            }

            // Sắp xếp
            switch (sortOrder.ToLower())
            {
                case "price_asc":
                    books = books.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.Price);
                    break;
                case "newest":
                default:
                    books = books.OrderByDescending(b => b.BookId); // Sách mới nhất đầu tiên
                    break;
            }

            // Nhóm sách theo danh mục để hiển thị theo nhóm trong view
            var booksByCategory = await books
                .GroupBy(b => b.Category)
                .ToListAsync();

            return View(booksByCategory);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Lấy danh sách đánh giá
            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Kiểm tra xem khách hàng có quyền đánh giá không
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            bool canReview = false;
            if (customerId != null)
            {
                canReview = await _context.OrderDetails
                    .Include(od => od.Order)
                    .AnyAsync(od => od.BookId == id && od.Order.CustomerId == customerId && od.Order.Status == "Tiền mặt" || od.Order.Status == "Chuyển khoản");
            }

            ViewBag.Reviews = reviews;
            ViewBag.CanReview = canReview;

            return View(book);
        }

        // POST: Books/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int BookId, int CustomerId, int Rating, string Comment)
        {
            var customerIdFromSession = HttpContext.Session.GetInt32("CustomerId");
            if (customerIdFromSession == null || customerIdFromSession != CustomerId)
            {
                TempData["Error"] = "Vui lòng đăng nhập để gửi bình luận.";
                return RedirectToAction("Details", new { id = BookId });
            }

            // Kiểm tra xem khách hàng đã mua sách chưa
            var canReview = await _context.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.BookId == BookId && od.Order.CustomerId == CustomerId && (od.Order.Status == "Tiền mặt" || od.Order.Status == "Chuyển khoản"));

            if (!canReview)
            {
                TempData["Error"] = "Bạn cần mua sách này để gửi bình luận.";
                return RedirectToAction("Details", new { id = BookId });
            }

            // Kiểm tra xem khách hàng đã đánh giá chưa
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookId == BookId && r.CustomerId == CustomerId);

            if (existingReview != null)
            {
                TempData["Error"] = "Bạn đã gửi bình luận cho sách này.";
                return RedirectToAction("Details", new { id = BookId });
            }

            var review = new Review
            {
                BookId = BookId,
                CustomerId = CustomerId,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.Now
            };

            if (ModelState.IsValid)
            {
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bình luận của bạn đã được gửi.";
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("Details", new { id = BookId });
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,BookTitle,Author,Description,Price,StockQuantity,OrderedQuantity,CategoryId,PublisherId,Status,CoverImageUrl")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", book.CategoryId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", book.CategoryId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,BookTitle,Author,Description,Price,StockQuantity,OrderedQuantity,CategoryId,PublisherId,Status,CoverImageUrl")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", book.CategoryId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
