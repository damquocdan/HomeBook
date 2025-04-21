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
        public async Task<IActionResult> Index()
        {
            var homeBookContext = _context.Books.Include(b => b.Category).Include(b => b.Publisher);
            return View(await homeBookContext.ToListAsync());
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
