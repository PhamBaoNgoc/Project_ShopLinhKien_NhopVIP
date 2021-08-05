using ShopLinhKien.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ShopLinhKien.Areas.Admin.Controllers
{
    public class LeaseItemController : Controller
    {
        private ShopLinhKienDbContext db = new ShopLinhKienDbContext();
        // GET: Admin/LeaseCategory
        public ActionResult Index()
        {
            var list = db.leaseItems.Where(m => m.Status > 0).ToList();
            return View(list);
        }
        public ActionResult Create()
        {
            ViewBag.listCate = db.leaseCategories.Where(m => m.Status != 0).ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(leaseItem leaseItem)
        {

            if (ModelState.IsValid)
            {
                leaseItem.Order = 1;
                //category
                db.leaseItems.Add(leaseItem);
                db.SaveChanges();
                Message.set_flash("Thêm  thành công", "success");
                return RedirectToAction("index");
            }
            ViewBag.listCate = db.leaseCategories.Where(m => m.Status != 0).ToList();
            return View(leaseItem);
        }
        public ActionResult Edit(int? id)
        {
            ViewBag.listCate = db.leaseCategories.Where(m => m.Status != 0).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            leaseItem leaseItem = db.leaseItems.Find(id);
            if (leaseItem == null)
            {
                return HttpNotFound();
            }
            return View(leaseItem);
        }
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(leaseItem leaseItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(leaseItem).State = EntityState.Modified;
                Message.set_flash("Sửa  thành công", "success");
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Message.set_flash("Sửa thất bại", "success");
            return View(leaseItem);
        }
    }
}