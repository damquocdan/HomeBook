using HomeBook.Areas.HomeBook.Controllers;
using HomeBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return View();
        }
    }

}
