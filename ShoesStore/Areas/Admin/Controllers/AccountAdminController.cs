using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using ShoesStore.Models.Authentication;
using System.Diagnostics;

namespace ShoesStore.Controllers
{
    [Area("Admin")]
    public class AccountAdminController : Controller
    {
        ShoesDbContext db = new ShoesDbContext();

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Email") != null || (HttpContext.Session.GetString("Loaitk") != "2" && HttpContext.Session.GetString("Loaitk") != "1"))
            {   //Check co tk hoac co phai la admin ko
                return View();
            }
            else
            {
                Debug.WriteLine("In login GET Redirect");
                return RedirectToAction("Index", "HomeAdmin");
                //Có rồi thì đi đến home admin 
            }
        }
        [HttpPost]
        public IActionResult Login(Taikhoan taikhoan)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                var u = db.Taikhoans
                    .Select(x => new { x.Email, x.Matkhau, x.Loaitk })
                    .FirstOrDefault(x => x.Email.Equals(taikhoan.Email) 
                        && x.Matkhau.Equals(taikhoan.Matkhau)
                        && (x.Loaitk == 2 || x.Loaitk == 1));

                if (u != null)
                {
                    HttpContext.Session.SetString("Email", u.Email);
                    HttpContext.Session.SetString("Loaitk", u.Loaitk.ToString());
                    return RedirectToAction("Index", "HomeAdmin");
                }
            }
            return View(taikhoan);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Login", "AccountAdmin");
        }	


	}
}
