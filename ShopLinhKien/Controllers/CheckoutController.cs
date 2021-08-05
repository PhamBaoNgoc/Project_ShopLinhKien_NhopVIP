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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopLinhKien.Controllers
{
    public class CheckoutController : BaseController
    {
        private const string SessionCart = "SessionCart";
        ShopLinhKienDbContext db = new ShopLinhKienDbContext();
        public ActionResult Index()
        {
            Muser sessionUser = (Muser)Session[Common.CommonConstants.USER_SESSION];
            ViewBag.sessionUser = sessionUser;
            var cart = Session[SessionCart];
            var list = new List<Cart_item>();
            if (cart != null)
            {
                list = (List<Cart_item>)cart;
            }
            return View(list);

        }
        [HttpPost]
        public ActionResult Index(Morder order)
        {

            Random rand = new Random((int)DateTime.Now.Ticks);
            int numIterations = 0;
            numIterations = rand.Next(1, 100000);
            DateTime time = DateTime.Now;
            string orderCode = numIterations + "" + time;
            string sumOrder = Request["sumOrder"];
            string payment_method = Request["option_payment"];
            // Neu Ship COde
            if (payment_method.Equals("COD"))
            {
                // cap nhat thong tin sau khi dat hang thanh cong

                saveOrder(order, "COD", 2, orderCode);
                var cart = Session[SessionCart];
                var list = new List<Cart_item>();
                ViewBag.cart = (List<Cart_item>)cart;
                Session["SessionCart"] = null;
                var listProductOrder = db.Orderdetails.Where(m => m.orderid == order.ID);
                string mailBody = renderHtmlEmail(order);
                SendEmail(order.deliveryemail, mailBody);
                return View("payment");
            }
            //Neu Thanh toan Ngan Luong
            else if (payment_method.Equals("NL"))
            {
                string str_bankcode = Request["bankcode"];
                RequestInfo info = new RequestInfo();
                info.Merchant_id = nganluongInfo.Merchant_id;
                info.Merchant_password = nganluongInfo.Merchant_password;
                info.Receiver_email = ShopLinhKien.nganluonAPI.nganluongInfo.Receiver_email;
                info.cur_code = "vnd";
                info.bank_code = str_bankcode;
                info.Order_code = orderCode;
                info.Total_amount = sumOrder;
                info.fee_shipping = "0";
                info.Discount_amount = "0";
                info.order_description = "Thanh toán ngân lượng cho đơn hàng";
                info.return_url = nganluongInfo.return_url;
                info.cancel_url = nganluongInfo.cancel_url;
                info.Buyer_fullname = order.deliveryname;
                info.Buyer_email = order.deliveryemail;
                info.Buyer_mobile = order.deliveryphone;
                APICheckoutV3 objNLChecout = new APICheckoutV3();
                ResponseInfo result = objNLChecout.GetUrlCheckout(info, payment_method);
                // neu khong gap loi gi
                if (result.Error_code == "00")
                {
                    saveOrder(order, "Cổng thanh toán Ngân Lượng", 2, orderCode);
                    // chuyen sang trang ngan luong
                    return Redirect(result.Checkout_url);
                }
                else
                {
                    ViewBag.errorPaymentOnline = result.Description;
                    return View("payment");
                }

            }
            //Neu Thanh Toán ATM online
            else if (payment_method.Equals("ATM_ONLINE"))
            {
                string str_bankcode = Request["bankcode"];
                RequestInfo info = new RequestInfo();
                info.Merchant_id = nganluongInfo.Merchant_id;
                info.Merchant_password = nganluongInfo.Merchant_password;
                info.Receiver_email = nganluongInfo.Receiver_email;
                info.cur_code = "vnd";
                info.bank_code = str_bankcode;
                info.Order_code = orderCode;
                info.Total_amount = sumOrder;
                info.fee_shipping = "0";
                info.Discount_amount = "0";
                info.order_description = "Thanh toán ngân lượng cho đơn hàng";
                info.return_url = nganluongInfo.return_url;
                info.cancel_url = nganluongInfo.cancel_url;
                info.Buyer_fullname = order.deliveryname;
                info.Buyer_email = order.deliveryemail;
                info.Buyer_mobile = order.deliveryphone;
                APICheckoutV3 objNLChecout = new APICheckoutV3();
                ResponseInfo result = objNLChecout.GetUrlCheckout(info, payment_method);
                // neu khong gap loi gi
                if (result.Error_code == "00")
                {
                    saveOrder(order, "ATM Online qua ngân lượng", 2, orderCode);
                    return Redirect(result.Checkout_url);
                }
                else
                {
                    ViewBag.errorPaymentOnline = result.Description;
                    return View("payment");
                }
            }
            return View("payment");
        }
        //Khi huy thanh toán Ngan Luong
        public ActionResult cancel_order()
        {

            return View("cancel_order");
        }
        //Khi thanh toán Ngan Luong XOng
        public ActionResult confirm_orderPaymentOnline()
        {

            String Token = Request["token"];
            RequestCheckOrder info = new RequestCheckOrder();
            info.Merchant_id = nganluongInfo.Merchant_id;
            info.Merchant_password = nganluongInfo.Merchant_password;
            info.Token = Token;
            APICheckoutV3 objNLChecout = new APICheckoutV3();
            ResponseCheckOrder result = objNLChecout.GetTransactionDetail(info);
            if (result.errorCode == "00")
            {
                var cart = Session[SessionCart];
                var list = new List<Cart_item>();
                ViewBag.cart = (List<Cart_item>)cart;
                Session["SessionCart"] = null;
                var OrderInfo = db.Orders.OrderByDescending(m => m.ID).FirstOrDefault();
                ViewBag.name = OrderInfo.deliveryname;
                ViewBag.email = OrderInfo.deliveryemail;
                ViewBag.address = OrderInfo.deliveryaddress;
                ViewBag.code = OrderInfo.code;
                ViewBag.phone = OrderInfo.deliveryphone;
                OrderInfo.StatusPayment = 1;
                db.Entry(OrderInfo).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.paymentStatus = OrderInfo.StatusPayment;
                ViewBag.Methodpayment = OrderInfo.deliveryPaymentMethod;

                string mailBody = renderHtmlEmail(OrderInfo);
                SendEmail(OrderInfo.deliveryemail, mailBody);

                return View("payment");
            }
            else
            {
                ViewBag.status = false;
            }

            return View("confirm_orderPaymentOnline");
        }

        //function ssave order when order success!
        public void saveOrder(Morder order, string paymentMethod, int StatusPayment, string ordercode)
        {
            var cart = Session[SessionCart];
            var list = new List<Cart_item>();
            if (cart != null)
            {
                list = (List<Cart_item>)cart;
            }

            if (ModelState.IsValid)
            {

                order.code = ordercode;
                order.userid = 1;
                order.deliveryPaymentMethod = paymentMethod;
                order.StatusPayment = StatusPayment;
                order.created_ate = DateTime.Now;
                order.updated_by = 1;
                order.updated_at = DateTime.Now;
                order.updated_by = 1;
                order.status = 2;
                order.exportdate = DateTime.Now;
                db.Orders.Add(order);
                db.SaveChanges();
                ViewBag.name = order.deliveryname;
                ViewBag.email = order.deliveryemail;
                ViewBag.address = order.deliveryaddress;
                ViewBag.code = order.code;
                ViewBag.phone = order.deliveryphone;
                Mordersdetail orderdetail = new Mordersdetail();

                foreach (var item in list)
                {
                    float price = 0;
                    int sale = (int)item.product.pricesale;
                    if (sale > 0)
                    {
                        price = (float)item.product.price - (int)item.product.price / 100 * (int)sale * item.quantity;
                    }
                    else
                    {
                        price = (float)item.product.price * (int)item.quantity;
                    }
                    orderdetail.orderid = order.ID;
                    orderdetail.productid = item.product.ID;
                    orderdetail.priceSale = (int)item.product.pricesale;
                    orderdetail.price = item.product.price;
                    orderdetail.quantity = item.quantity;
                    orderdetail.amount = price;

                    db.Orderdetails.Add(orderdetail);
                    db.SaveChanges();
                    var updatedProduct = db.Products.Find(item.product.ID);
                    updatedProduct.number = (int)updatedProduct.number - (int)item.quantity;
                    db.Products.Attach(updatedProduct);
                    db.Entry(updatedProduct).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }
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
        string renderHtmlEmail(Morder order)
        {
            string statusPayemntS = "Chưa thanh toán";
            if (order.StatusPayment == 1)
            {
                statusPayemntS = "Đã thanh toán";
            }
            string mailBody = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/mailTemplateProduct.html"));

            mailBody = mailBody.Replace("{{name}}", order.deliveryname);
            mailBody = mailBody.Replace("{{orderCode}}", order.ID.ToString());
            mailBody = mailBody.Replace("{{email}}", order.deliveryemail);
            mailBody = mailBody.Replace("{{phone}}", order.deliveryphone);
            mailBody = mailBody.Replace("{{address}}", order.deliveryaddress);
            mailBody = mailBody.Replace("{{statuspayment}}", order.deliveryPaymentMethod);
            mailBody = mailBody.Replace("{{Methodpayment}}", statusPayemntS);
            mailBody = mailBody.Replace("{{created_ate}}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt"));

            var listOrder = db.Orderdetails.Where(m => m.orderid == order.ID).ToList();
            var product = new List<Mproduct>();

            var listOrderJoinProduct = from or in listOrder
                                       join pr in db.Products on or.productid equals pr.ID
                                       select new
                                       {
                                           name = pr.name,
                                           price = pr.price,
                                           priceSale = pr.pricesale,
                                           quantity = or.quantity,
                                           amount = or.amount,
                                       };
            listOrderJoinProduct = listOrderJoinProduct.ToList();

            string html1 = @" <table class='shopping-cart-table table'>
                <thead>
                    <tr>
                        <th>Tên sản phẩm</th>
                        <th class='text-center'>Giá sản phẩm</th>
                        <th class='text-center'>Số lượng</th>
                        <th class='text-center'>Tổng</th>
                    </tr>
                </thead>
                <tbody>";

            float sum = 0;
            var htmlItem = string.Empty;
            foreach (var iten in listOrderJoinProduct)
            {
                string prince1 = iten.price.ToString("N0");
                string amount1 = iten.amount.ToString("N0");
                sum = sum + (int)iten.amount;
                htmlItem +=
                    @"<tr>
                        <td class='details'>" + iten.name + @"</td>
                        <td>" + prince1 + @"VND (giá khuyến mãi) </td>
                        <td class='price text-center'>" + iten.quantity + @"</td>
                        <td class='price text-center'>" + amount1 + @"</td>                      
                    </tr></tbody>";
            }
            string summ = sum.ToString("N0");
            string html2 = @" 
                  
                    <tr>
                        <th class='empty' colspan='3'></th>
                        <th>Phí vận chuyển</th>
                        <td colspan='2'>Free Shipping</td>
                    </tr>
                    <tr>
                        <th class='empty' colspan='3'></th>
                        <th>Tổng tiền</th>
                        <th colspan='2' class='total'>" + summ + @"VND</th>
                    </tr>
                </tfoot>
            </table>";
            mailBody = mailBody.Replace("{{rderDetail}}", html1+ htmlItem+ html2);
            return mailBody;
        }
    }
}