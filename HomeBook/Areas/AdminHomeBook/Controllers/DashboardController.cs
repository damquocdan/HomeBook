using HomeBook.Areas.HomeBook.Controllers;
using HomeBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HomeBook.Areas.AdminHomeBook.Controllers {

    public class DashboardController : BaseController
    {
        private readonly HomeBookContext _context;

        public DashboardController(HomeBookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Total Statistics
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Statistics.SumAsync(s => s.TotalRevenue);
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalBooks = await _context.Books.CountAsync();

            // Revenue Over Time (Last 7 Days)
            var revenueData = await _context.Statistics
                .Where(s => s.ReportDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-6)))
                .OrderBy(s => s.ReportDate)
                .Select(s => new { s.ReportDate, s.TotalRevenue })
                .ToListAsync();

            ViewBag.RevenueLabels = JsonConvert.SerializeObject(revenueData.Select(r => r.ReportDate.ToString("dd/MM/yyyy")).ToArray());
            ViewBag.RevenueValues = JsonConvert.SerializeObject(revenueData.Select(r => r.TotalRevenue).ToArray());

            // Top 5 Best-Selling Books
            var topBooks = await _context.OrderDetails
                .GroupBy(od => od.Book)
                .Select(g => new
                {
                    BookTitle = g.Key.BookTitle,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(5)
                .ToListAsync();

            ViewBag.TopBooksLabels = JsonConvert.SerializeObject(topBooks.Select(b => b.BookTitle).ToArray());
            ViewBag.TopBooksValues = JsonConvert.SerializeObject(topBooks.Select(b => b.TotalSold).ToArray());

            return View();
        }
    }
}


