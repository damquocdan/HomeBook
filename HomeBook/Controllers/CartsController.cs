using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBook.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HomeBook.Controllers
{
    public class CartsController : Controller
    {
        private readonly HomeBookContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public CartsController(HomeBookContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Carts/Index
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "LoginC");
            }

            var carts = _context.Carts
                .Include(c => c.Book)
                .Where(c => c.CustomerId == customerId);
            return View(await carts.ToListAsync());
        }

        // POST: Carts/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để cập nhật giỏ hàng." });
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.CustomerId == customerId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            if (quantity < 1)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0.", currentQuantity = cart.Quantity });
            }

            cart.Quantity = quantity;
            _context.Update(cart);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật số lượng thành công!" });
        }

        // POST: Carts/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xóa sản phẩm.";
                return RedirectToAction("Index", "LoginC");
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == id && c.CustomerId == customerId);
            if (cart == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Carts/Add/5
        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để sử dụng chức năng này.";
                return RedirectToAction("Index", "LoginC");
            }

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.BookId == id && c.CustomerId == customerId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                var cart = new Cart
                {
                    CustomerId = customerId.Value,
                    BookId = id,
                    Quantity = 1
                };
                await _context.Carts.AddAsync(cart);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sách đã được thêm vào giỏ hàng!";
            return RedirectToAction("Index", "Books");
        }

        // GET: Carts/ConfirmAddress
        [HttpGet]
        public async Task<IActionResult> ConfirmAddress()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Index", "LoginC");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Books");
            }

            return View(customer);
        }

        // POST: Carts/PlaceOrderWithAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderWithAddress(string newAddress, string paymentMethod)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Index", "LoginC");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Books");
            }

            if (!string.IsNullOrEmpty(newAddress))
            {
                customer.Address = newAddress;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }

            var cartItems = await _context.Carts
                .Include(c => c.Book)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            var totalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity);
            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Đang xử lý"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                if (item.Book == null)
                {
                    TempData["Error"] = "Một số sản phẩm trong giỏ hàng không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Price = item.Book.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            await _context.SaveChangesAsync();

            if (paymentMethod == "MoMo")
            {
                var paymentUrl = await CreateMoMoPayment(order.OrderId, totalAmount);
                if (!string.IsNullOrEmpty(paymentUrl))
                {
                    HttpContext.Session.SetInt32("PendingOrderId", order.OrderId);
                    return Redirect(paymentUrl);
                }
                else
                {
                    TempData["Error"] = "Không thể tạo yêu cầu thanh toán MoMo.";
                    return RedirectToAction(nameof(ConfirmAddress));
                }
            }
            else
            {
                order.Status = "Tiền mặt";
                _context.Orders.Update(order);
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đơn hàng đã được đặt thành công.";
                return RedirectToAction("Index", "Books");
            }
        }

        // GET: Carts/MoMoCallback
        [HttpGet]
        public async Task<IActionResult> MoMoCallback(string orderId, string resultCode, string message)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => "MM" + o.OrderId == orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(ConfirmAddress));
            }

            if (resultCode == "0")
            {
                order.Status = "Chuyển khoản";
                _context.Orders.Update(order);

                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == order.CustomerId)
                    .ToListAsync();
                _context.Carts.RemoveRange(cartItems);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thanh toán MoMo thành công. Đơn hàng đã được đặt.";
                return RedirectToAction("Index", "Books");
            }
            else
            {
                TempData["Error"] = $"Thanh toán MoMo thất bại: {message}";
                return RedirectToAction(nameof(ConfirmAddress));
            }
        }

        // Hàm tạo yêu cầu thanh toán MoMo
        private async Task<string> CreateMoMoPayment(int orderId, decimal amount)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMO_ATM_DEV"; // Thay bằng partnerCode của bạn
            string accessKey = "w9gEg8bjA2AM2Cvr"; // Thay bằng accessKey của bạn
            string secretKey = "mD9QAVi4cm9N844jh5Y2tqjWaaJoGVFM"; // Thay bằng secretKey của bạn
            string orderInfo = $"Thanh toán đơn hàng #{orderId}";
            string redirectUrl = "https://localhost:7032/Carts/MoMoCallback"; // Thay bằng URL callback của bạn
            string ipnUrl = "https://localhost:7032/Carts/MoMoCallback"; // Thay bằng URL IPN của bạn
            string requestId = Guid.NewGuid().ToString();
            string orderIdStr = $"MM{orderId}";
            string amountStr = ((int)amount).ToString();

            var rawData = $"accessKey={accessKey}&amount={amountStr}&extraData=&ipnUrl={ipnUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType=payWithATM";
            var signature = HmacSha256(secretKey, rawData);

            var requestData = new
            {
                partnerCode,
                partnerName = "HomeBook",
                storeId = "BookStore",
                requestId,
                amount = amountStr,
                orderId = orderIdStr,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang = "vi",
                requestType = "payWithATM",
                autoCapture = true,
                extraData = "",
                signature
            };

            using var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                return responseData.payUrl;
            }

            return null;
        }

        // Hàm tạo chữ ký HMAC SHA256
        private string HmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}