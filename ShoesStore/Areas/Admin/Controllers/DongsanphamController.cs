using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoesStore.Areas.Admin.InterfaceRepositories;
using ShoesStore.Models;
using ShoesStore.Models.Authentication;
using System.Diagnostics;
using System.Linq;

namespace ShoesStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthenticationM_S]
    public class DongsanphamController : Controller
    {
        private readonly IDongsanphamAdmin _dongsanphamRepo;

        private readonly ILoaiAdmin _loairepo;

        private readonly ShoesDbContext _db;

        public DongsanphamController(IDongsanphamAdmin dongsanphamRepo, ILoaiAdmin loairepo, ShoesDbContext db)
        {
            _dongsanphamRepo = dongsanphamRepo;
            _loairepo = loairepo;
            _db = db;
        }

        public IActionResult Index(string searchString)
        {
            var query = _db.Dongsanphams
                .Include(d => d.MaloaiNavigation)
                .Include(d => d.Sanphams)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(d => 
                    d.Tendongsp.ToLower().Contains(searchString) || 
                    d.MaloaiNavigation.Tenloai.ToLower().Contains(searchString)
                );
            }

            var dongsanphams = query.ToList();
            return View(dongsanphams);
        }

        private SelectList GetSelectListItems()
        {
            var loaiList = _loairepo.GetAllLoai().ToList();
            return new SelectList(loaiList, "Maloai", "Tenloai");
        }

        public IActionResult Create()
        {

            ViewBag.Selectloai = GetSelectListItems();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Dongsanpham dongsanpham)
        {
            dongsanpham.MaloaiNavigation = null;
            var loaiList = _loairepo.GetAllLoai().ToList();
            ViewBag.Selectloai = new SelectList(loaiList, "Maloai", "Tenloai", dongsanpham.Maloai);
            if (ModelState.IsValid)
            {
                _dongsanphamRepo.AddDongsanpham(dongsanpham);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Selectloai = GetSelectListItems();
            return View(dongsanpham);
        }

        public IActionResult Edit(int id)
        {
            var dongsanpham = _dongsanphamRepo.GetDongsanphamById(id);
            var loaiList = _loairepo.GetAllLoai().ToList();
            ViewBag.Selectloai = new SelectList(loaiList, "Maloai", "Tenloai", dongsanpham.Maloai);

            return View(dongsanpham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Dongsanpham dongsanpham, int id)
        {
            if (ModelState.IsValid)
            {
                _dongsanphamRepo.UpdateDongsanpham(dongsanpham, id);
                return RedirectToAction(nameof(Index));
            }
            return View(dongsanpham);
        }



        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _dongsanphamRepo.DeleteDongsanpham(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
