using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ShoesStore.ViewModels;
using Microsoft.EntityFrameworkCore;
using ShoesStore.InterfaceRepositories;
using ShoesStore.Repositories;
using ShoesStore.Services;
using Microsoft.Extensions.Logging;

namespace ShoesStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShoesDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ShoesDbContext db, IEmailService emailService, ILogger<AccountController> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Register()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (HttpContext.Session.GetString("Email") != null && HttpContext.Session.GetString("Loaitk") == "0")
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang Home
                return RedirectToAction("Index", "Home");
            }

            // Nếu chưa đăng nhập, hiển thị trang đăng ký
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            // Kiểm tra email có đúng định dạng "@gmail.com" hay không
            if (string.IsNullOrEmpty(model.Khachhang.Email) || !model.Khachhang.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Khachhang.Email", "Email must end with  @gmail.com");
                return View(model);
            }

            // Kiểm tra email đã tồn tại trong cơ sở dữ liệu
            Taikhoan existEmail = _db.Taikhoans.FirstOrDefault(x => x.Email == model.Khachhang.Email);
            if (existEmail != null)
            {
                ModelState.AddModelError("Khachhang.Email", "Email already exists");
                return View(model);
            }

            // Tạo đối tượng Taikhoan mới
            Taikhoan newTk = new Taikhoan
            {
                Email = model.Khachhang.Email,
                Matkhau = model.Taikhoan.Matkhau,
                Loaitk = 0
            };

            model.Taikhoan = newTk;
            model.Khachhang.EmailNavigation = newTk;

            // Kiểm tra ModelState sau khi gán các giá trị vào đối tượng
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Thêm tài khoản và khách hàng vào cơ sở dữ liệu
                _db.Taikhoans.Add(newTk);
                _db.Khachhangs.Add(model.Khachhang);
                _db.SaveChanges();

                // Lưu thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Registered successfully! Please log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo lỗi nếu gặp sự cố trong quá trình lưu dữ liệu
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }
	
    // GET: /Account/Login
    public IActionResult Login(string backToPage = "")
        {
            if(backToPage != "")
            {
                ViewBag.backToPage = backToPage;
                
            }
          
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (HttpContext.Session.GetString("Email") == null || HttpContext.Session.GetString("Loaitk") != "0")
            {
                // Nếu chưa đăng nhập, hiển thị trang đăng nhập
                return View();
            }
            else
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang Home
                return RedirectToAction("Index", "Home");
            }
        }
        
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Taikhoan taikhoan, string backToPage = "")
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (HttpContext.Session.GetString("Email") == null || HttpContext.Session.GetString("Loaitk") != "0")
            {
                // Tìm kiếm tài khoản trong cơ sở dữ liệu
                var user = _db.Taikhoans.FirstOrDefault(x => x.Email == taikhoan.Email && x.Matkhau == taikhoan.Matkhau && x.Loaitk == 0);

                if (user != null)
                {
                    // Lưu thông tin người dùng vào Session
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("Loaitk", user.Loaitk.ToString());

                    if (backToPage == "thanhtoan")
                    {
                        return RedirectToAction("ThanhToan", "PhieuMua");
                    }
                    if (backToPage == "comment")
                    {
                        int masp = HttpContext.Session.GetInt32("Masp") ?? 0;
                        int madongsp = _db.Sanphams.Find(masp).Madongsanpham;

                        return RedirectToAction("HienThiSanpham", "SanPham", new { madongsanpham = madongsp, masp =masp});
                    }
                    //if (comment == 1)
                    //{
                    //	return RedirectToAction("HienThiSanPham", "SanPham");
                    //}
                    // Chuyển hướng đến trang Home
                    return RedirectToAction("Index", "Home");
                }
            }
            if (backToPage != "")
            {
                ViewBag.backToPage = backToPage;
            }
           
            return View(taikhoan);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Xóa thông tin người dùng khỏi Session
            HttpContext.Session.Remove("Email");

            // Chuyển hướng đến trang Login
            return RedirectToAction("Login", "Account");
        }

        public IActionResult UserProfile()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (HttpContext.Session.GetString("Email") == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang Login
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin tài khoản khách hàng từ Session
            string userEmail = HttpContext.Session.GetString("Email");
            var user = _db.Taikhoans
                            .Include(t => t.Khachhang)
                            .FirstOrDefault(x => x.Email == userEmail);

            if (user != null)
            {
                return View(user.Khachhang);
            }

            // Nếu không tìm thấy thông tin, chuyển hướng về trang Home
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Getuserprofile()
        {
            return ViewComponent("UserProfile");
        }

        public IActionResult AddressBook()
        {
            return ViewComponent("AddressBook");
        }

        // GET: /Account/ChangeProfile
        public IActionResult ChangeProfile()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            ////if (HttpContext.Session.GetString("Email") == null)
            ////{
            ////    // Nếu chưa đăng nhập, chuyển hướng đến trang Login
            ////    return RedirectToAction("Login", "Account");
            ////}

            // Lấy thông tin tài khoản khách hàng từ Session
            string userEmail = HttpContext.Session.GetString("Email") ;
            var user = _db.Taikhoans
                            .Include(t => t.Khachhang)
                            .FirstOrDefault(x => x.Email == userEmail);

            if (user != null)
            {
                return View(user.Khachhang);
            }

            // Nếu không tìm thấy thông tin, chuyển hướng về trang Home
            return RedirectToAction("Index", "Home");
        }

        //[ValidateAntiForgeryToken]
        public IActionResult ChangeProfileUpdate(string tenkh, string sdt, bool gioitinh, DateTime? ngaysinh)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            //if (HttpContext.Session.GetString("Email") == null)
            //{
            //    // Nếu chưa đăng nhập, chuyển hướng đến trang Login
            //    return RedirectToAction("Login", "Account");
            //}

            // Lấy email của người dùng từ Session
            string userEmail = HttpContext.Session.GetString("Email");

            // Tìm thông tin khách hàng dựa trên email
            var customer = _db.Khachhangs.FirstOrDefault(kh => kh.Email == userEmail);

            if (customer != null)
            {
                // Cập nhật thông tin khách hàng
                customer.Tenkh = tenkh;
                customer.Sdt = sdt;
                customer.Gioitinh = gioitinh;
                customer.Ngaysinh = ngaysinh;

                _db.Khachhangs.Update(customer);
                _db.SaveChanges();

                // Trả về JSON hoặc thông báo thành công
                return PartialView("PartialViewProfileInfo", customer);
            }

            // Nếu không tìm thấy thông tin, trả về thông báo lỗi
            return Json(new { success = false });
        }

        public IActionResult OrderHistory()
        {
            return ViewComponent("OrderHistory");
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            if (HttpContext.Session.GetString("Email") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/ForgotPassword 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Chỉ kiểm tra email có tồn tại không
            var user = await _db.Taikhoans
                .Select(x => new { x.Email, x.Loaitk })
                .FirstOrDefaultAsync(x => x.Email == model.Email && x.Loaitk == 0);

            if (user == null)
            {
                TempData["ErrorMessage"] = "No account found with this email address.";
                return View(model);
            }

            // Chỉ hiển thị thông báo thành công
            TempData["SuccessMessage"] = "Password reset instructions have been sent to your email.";
            return View(model);
        }

        // GET: /Account/ResetPassword
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _db.Taikhoans.FirstOrDefaultAsync();

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid or expired password reset link.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Taikhoans.FirstOrDefaultAsync();

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid or expired password reset link.";
                return RedirectToAction("Login");
            }

            // Cập nhật mật khẩu mới
            user.Matkhau = model.NewPassword;
            
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully. Please login with your new password.";
            return RedirectToAction("Login");
        }
    }
}
