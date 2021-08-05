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
    public class LeaseCategoryController : Controller
    {
        private ShopLinhKienDbContext db = new ShopLinhKienDbContext();
        // GET: Admin/LeaseCategory
        public ActionResult Index()
        {
            var list = db.leaseCategories.Where(m => m.Status > 0).ToList();
            return View(list);
        }
        public ActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create(leaseCategory leaseCategory)
        {

            if (ModelState.IsValid)
            {
                //category
                string slug = Mystring.ToSlug(leaseCategory.Name.ToString());
                leaseCategory.created_at = DateTime.Now;              
                db.leaseCategories.Add(leaseCategory);
                db.SaveChanges();            
                Message.set_flash("Thêm  thành công", "success");
                return RedirectToAction("index");
            }     
            return View(leaseCategory);
        }
        public ActionResult Edit(int? id)
        {
            ViewBag.listCate = db.Categorys.Where(m => m.status != 0).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            leaseCategory leaseCategory = db.leaseCategories.Find(id);
            if (leaseCategory == null)
            {
                return HttpNotFound();
            }
            return View(leaseCategory);
        }

      [HttpPost]
        public ActionResult Edit(leaseCategory leaseCategory)
        {
            if (ModelState.IsValid)
            {
                string slug = Mystring.ToSlug(leaseCategory.Name.ToString());
                leaseCategory.created_at = DateTime.Now;
                db.Entry(leaseCategory).State = EntityState.Modified;  
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Message.set_flash("Sửa thất bại", "success");
            return View(leaseCategory);
        }
    }
}