using ShopLinhKien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ShopLinhKien.Areas.Admin.Controllers
{
    public class LeaseOrderController : Controller
    {
        private ShopLinhKienDbContext db = new ShopLinhKienDbContext();

        public ActionResult Index()
        {
            var list = db.LeaseOrders.ToList();
            return View(list);
        }
        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var info = db.LeaseOrders.Where(m => m.ID == id).FirstOrDefault();
            ViewBag.leaseItem = db.leaseItems.Where(m => m.ID == info.LeaseItemID).FirstOrDefault();
            return View("Detail", info);
        }
    }
}