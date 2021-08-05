using API_NganLuong;
using ShopLinhKien.Library;
using ShopLinhKien.Models;
using ShopLinhKien.nganluonAPI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ShopLinhKien.Controllers
{
    public class leaseOrdersController : BaseController
    {
        ShopLinhKienDbContext db = new ShopLinhKienDbContext();
        // GET: leaseOrders
        public ActionResult leaseOrder(FormCollection fc)
        {
            Muser sessionUser = (Muser)Session[Common.CommonConstants.USER_SESSION];
            if (sessionUser == null)
            {
                Message.set_flash("Vui lòng đăng nhập", "danger");
                return Redirect("~/");
            }
            ViewBag.ngayThue = int.Parse(fc["ngayThue"].ToString());
            int Id = int.Parse(fc["Id"].ToString());
            ViewBag.leaseId = Id;
            ViewBag.leaseItem = db.leaseItems.Find(Id);
            return View(sessionUser);
        }
        [HttpPost]
        public ActionResult leaseOrderSUBMIT(LeaseOrder leaseOrder, FormCollection fc)
        {

            Muser sessionUser = (Muser)Session[Common.CommonConstants.USER_SESSION];
            if (sessionUser == null)
            {
                Message.set_flash("Vui lòng đăng nhập", "danger");
                return Redirect("~/");
            }

            int leaseId = int.Parse(fc["LeaseId"].ToString());
            float sum = float.Parse(fc["sum"].ToString());
            int RentalPeriod = int.Parse(fc["RentalPeriod"].ToString());

            string payment_method = Request["option_payment"];
            string str_bankcode = Request["bankcode"];
            RequestInfo info = new RequestInfo();
            info.Merchant_id = nganluongInfo.Merchant_id;
            info.Merchant_password = nganluongInfo.Merchant_password;
            info.Receiver_email = ShopLinhKien.nganluonAPI.nganluongInfo.Receiver_email;
            info.cur_code = "vnd";
            info.bank_code = str_bankcode;
            info.Order_code = "KDJF4343";
            info.Total_amount = sum.ToString();
            info.fee_shipping = "0";
            info.Discount_amount = "0";
            info.order_description = "Thanh toán ngân lượng cho đơn hàng";
            info.return_url = nganluongInfo.return_url1;
            info.cancel_url = nganluongInfo.cancel_url1;
            info.Buyer_fullname = leaseOrder.Name;
            info.Buyer_email = leaseOrder.Emal;
            info.Buyer_mobile = leaseOrder.Phone;
            APICheckoutV3 objNLChecout = new APICheckoutV3();
            ResponseInfo result = objNLChecout.GetUrlCheckout(info, payment_method);
            // neu khong gap loi gi
            if (result.Error_code == "00")
            {
                leaseOrder.PaymentMethod = "Thanh toán: " + payment_method;
                leaseOrder.StatusPayment = 0;
                leaseOrder.LeaseItemID = leaseId;
                leaseOrder.UserId = sessionUser.ID;
                leaseOrder.TotalPrice = sum;
                leaseOrder.RentalPeriod = RentalPeriod;
                db.LeaseOrders.Add(leaseOrder);
                db.SaveChanges();
                Session["LeaseId"] = leaseId;
                Session["LeaseOdersId"] = leaseOrder.ID;

                // chuyen sang trang ngan luong
                return Redirect(result.Checkout_url);
            }
            else
            {
                ViewBag.errorPaymentOnline = result.Description;
                return View("cancel_order");
            }

        }
        //Khi huy thanh toán Ngan Luong
        public ActionResult cancel_order()
        {

            return View("cancel_order");
        }
        //Khi thanh toán Ngan Luong XOng
        public ActionResult confirm_orderPaymentOnline()
        {
           int leaseId = int.Parse(Session["LeaseId"].ToString());
           int LeaseOdersId = int.Parse(Session["LeaseOdersId"].ToString());
            ViewBag.leaseItem = db.leaseItems.Where(m=>m.ID == leaseId).FirstOrDefault();
            LeaseOrder lease = db.LeaseOrders.Where(m=>m.ID == LeaseOdersId).FirstOrDefault();
            lease.StatusPayment = 1;
            db.Entry(lease).State = EntityState.Modified;
            db.SaveChanges();
            string mailBody = renderHtmlEmail(lease, db.leaseItems.Find(leaseId));
            SendEmail(lease.Emal, mailBody);
            return View("checkOutComfin", lease);  
        }

        public void SendEmail(string CustomerEmail, string mailBody)
        {

            MailMessage mm = new MailMessage(Util.email, CustomerEmail);
            mm.Subject = "CHI TIẾT ĐƠN THUÊ DỊCH VỤ";
            mm.Body = mailBody;
            mm.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            /// Email dùng để gửi đi
            NetworkCredential nc = new NetworkCredential(Util.email, Util.password);
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = nc;
            smtp.Send(mm);
        }
        string renderHtmlEmail(LeaseOrder order, leaseItem leaseItem)
        {
            string statusPayemntS = "Chưa thanh toán";

            string mailBody = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/mailTemplate.html"));
          
            mailBody = mailBody.Replace("{{name}}", order.Name);
            mailBody = mailBody.Replace("{{orderCode}}", order.ID.ToString());
            mailBody = mailBody.Replace("{{email}}", order.Emal);
            mailBody = mailBody.Replace("{{phone}}", order.Phone);
            mailBody = mailBody.Replace("{{NgayThue}}", order.RentalPeriod.ToString());
            mailBody = mailBody.Replace("{{TienTrenNgay}}", leaseItem.Price.ToString("N0") + "VND");

            
            mailBody = mailBody.Replace("{{created_ate}}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt"));
            if (order.StatusPayment == 1)
            {
                statusPayemntS = "Đã thanh toán";
            }    
            mailBody = mailBody.Replace("{{statuspayment}}", order.StatusPayment.ToString());
            mailBody = mailBody.Replace("{{Methodpayment}}", statusPayemntS);
            mailBody = mailBody.Replace("{{total}}", order.TotalPrice.ToString("N0") + "VND");
            string htmlListItem = @"<div class='card '>
                    <h6 class='bg-cam font-weight-bold text-center border-bottom py-2 text-white'>Dịch vụ</h6>
                    <div class='card'>
                        <div class='card-body'>
                            <div class='card-title text-dark pt-1'><h5 class='font-weight-bold text-info text-center'>"+leaseItem.Name+@"</h5></div>
                            <div class='card-title text-dark pt-1'><h6 class='font-weight-bold text-info text-center'>"+ leaseItem.Description + @"</h6></div>
                            <div style = 'height:430px;' class='p-3'>" +
                                leaseItem.Content+ @"
                            </div>
                        </div>
                        </div>
                    </div>";
            mailBody = mailBody.Replace("{{OrderDetail}}", htmlListItem);
            return mailBody;
        }
    }
}