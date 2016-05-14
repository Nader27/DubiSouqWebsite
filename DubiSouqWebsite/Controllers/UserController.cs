using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DubiSouqWebsite.Models;
using System.IO;
using System.Web.Security;

namespace DubiSouqWebsite.Controllers
{
    public class UserController : Controller
    {
        private Entities db = new Entities();

        public object HttpStatusCode { get; private set; }

        //GET: /User
        public ActionResult Index()
        {
            if (Request.Cookies["user"] != null && Session["user"] == null)
            {
                string email = Request.Cookies["user"].Value;
                user us = db.users.Single(u => u.Email == email);
                Session["user"] = us;
            }
            if (Session["user"] == null)
                return RedirectToAction("Login");
            else
                return RedirectToAction("Home", "Home");
        }

        // GET: /User/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Request.Cookies["user"] != null && Session["user"] == null)
            {
                string email = Request.Cookies["user"].Value;
                user us = db.users.SingleOrDefault(u => u.Email == email);
                Session["user"] = us;
            }
            if (Session["user"] == null)
                return View();
            else
                return RedirectToAction("Home", "Home");
        }

        //POST: /User/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(BaseViewModels model)
        {
            ModelState.Remove("User.Active");
            ModelState.Remove("User.Mobile");
            ModelState.Remove("User.ConfirmPassword");
            ModelState.Remove("User.Picture");
            ModelState.Remove("User.Name");
            ModelState.Remove("User.Type_id");
            ModelState.Remove("address.Zipcode");
            ModelState.Remove("address.Address");
            ModelState.Remove("address.User_ID");
            ModelState.Remove("address.Country");
            ModelState.Remove("address.City");
            if (ModelState.IsValid)
            {
                user myUser = db.users.SingleOrDefault(u => u.Email == model.user.Email
                     && u.Password == model.user.Password && u.Active);
                if (myUser != null)
                {
                    if (model.RememberMe == true)
                    {
                        if (myUser.Type_id == 1)
                        {
                            HttpCookie cookie = Request.Cookies["user"];
                            cookie = new HttpCookie("user", myUser.Email);
                            Response.SetCookie(cookie);
                            Session["user"] = myUser;
                        }
                        else
                        {
                            HttpCookie cookie = Request.Cookies["admin"];
                            cookie = new HttpCookie("admin", myUser.Email);
                            Response.SetCookie(cookie);
                            Session["admin"] = myUser;
                        }
                    }
                    else
                    {
                        if (myUser.Type_id == 1)
                            Session["user"] = myUser;
                        else
                            Session["admin"] = myUser;
                    }
                    if (myUser.Type_id == 1)
                        return RedirectToAction("Home", "Home");
                    else
                        return RedirectToAction("Index", "Admin", new { Area = "Admin" });
                }
                else
                {
                    ModelState.AddModelError("", "Login data is incorrect!");
                }
            }
            return View(model);
        }

        //GET: /User/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Request.Cookies["user"] != null && Session["user"] == null)
            {
                string email = Request.Cookies["user"].Value;
                user us = db.users.Single(u => u.Email == email);
                Session["user"] = us;
            }
            if (Session["user"] == null)
                return View();
            else
                return RedirectToAction("Home", "Home");
        }

        //POST: /User/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(BaseViewModels model)
        {
            ModelState.Remove("User.Active");
            ModelState.Remove("User.Picture");
            ModelState.Remove("User.Type_id");
            if (ModelState.IsValid)
            {
                if (db.users.FirstOrDefault(m => m.Email == model.user.Email) == null)
                {
                    model.user.Active = true;
                    model.user.Type_id = 1;
                    model.user.Picture = "images/Profile/default.png";
                    model.user.Token = null;
                    db.users.Add(model.user);
                    db.SaveChanges();
                    model.address.User_ID = db.users.Max(m => m.ID);
                    db.addresses.Add(model.address);
                    db.SaveChanges();
                    int ID = db.users.Max(u => u.ID);
                    Session["user"] = db.users.Find(ID);
                    ReportModel.CreateUserReport(ID, 5, ID, model.user.Email);
                }
                else
                {
                    ModelState.AddModelError("user.Email", "Email Already used");
                    return View(model);
                }
                return RedirectToAction("Home", "Home");
            }
            return View(model);
        }

        //POST: /User/Logout
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            if (Request.Cookies["user"] != null)
            {
                var user = new HttpCookie("user")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = null
                };
                Response.SetCookie(user);
                Session["user"] = null;
            }
            else if (Session != null && Session["user"] != null)
            {
                Session["user"] = null;
            }
            if (Request.Cookies["admin"] != null)
            {
                var user = new HttpCookie("admin")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = null
                };
                Response.SetCookie(user);
                Session["admin"] = null;
            }
            else if (Session != null && Session["admin"] != null)
            {
                Session["admin"] = null;
            }
            return RedirectToAction("Home", "Home", new { Area = "" });
        }

        //GET: /User/Account
        public ActionResult Account(string id ="")
        {
            if (Session["user"] == null)
                return RedirectToAction("Login");
            switch (id)
            {
                case "profile":
                    ViewBag.Change = "ChangeMyProfile";
                    break;
                case "password":
                    ViewBag.Change = "ChangeMyPassword";
                    break;
                case "picture":
                    ViewBag.Change = "ChangeMyPicture";
                    break;
                case "email":
                    ViewBag.Change = "ChangeMyEmail";
                    break;
                case "address":
                    ViewBag.Change = "ChangeMyAddress";
                    break;
                default:
                    break;
            }
            BaseViewModels model = new BaseViewModels();
            user user = Session["user"] as user;
            address address = db.addresses.FirstOrDefault(a => a.User_ID == user.ID);
            model.user = user;
            model.address = address;
            return View(model);
        }

        public ActionResult MangeOffer(string id = "")
        {

            if (Session["user"] == null)
                return RedirectToAction("Login");
            switch (id)
            {
                case "add":
                    ViewBag.Change = "MakeOffer";
                    break;
                case "edit":
                    ViewBag.Change = "EditOffer";
                    break;
                case "picture":
                    ViewBag.Change = "OfferPicture";
                    break;
                case "delete":
                    ViewBag.Change = "DeleteOffer";
                    break;
                default:
                    break;
            }
            user user = Session["user"] as user;
            List<product> products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 2 && p.User_ID == user.ID).ToList();
            return View(products);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Offer(product product, string id = "")
        //{

        //    //if (Session["user"] == null)
        //    //    return RedirectToAction("Login");
        //    //switch (id)
        //    //{
        //    //    case "add":
        //    //        ViewBag.Change = "MakeOffer";
        //    //        if (MakeOffer(product))
        //    //            TempData["productsuccess"] = "Offer Added Successfully";
        //    //        break;
        //        //case "edit":
        //        //    ViewBag.Change = "EditOffer";
        //        //    if (EditOffer(product))
        //        //        TempData["passwordsuccess"] = "Offer Updated Successfully";
        //        //    break;
        //        //case "picture":
        //        //    ViewBag.Change = "OfferPicture";
        //        //    if (file != null)
        //        //    {
        //        //        if (!HttpPostedFileBaseExtensions.IsImage(file))
        //        //            ModelState.AddModelError("", "File is not an Image");
        //        //        if (ChangeMyPicture(product, file))
        //        //            TempData["picturesuccess"] = "Offer Picture Updated Successfully";
        //        //    }
        //        //    else
        //        //        ModelState.AddModelError("", "Select File to upload");
        //        //    break;
        //        //case "delete":
        //        //    ViewBag.Change = "DeleteOffer";
        //        //    if (db.users.FirstOrDefault(m => m.Email == model.user.Email) != null)
        //        //        ModelState.AddModelError("user.Email", "Email Already used");
        //        //    if (ChangeMyEmail(model.user))
        //        //        TempData["emailsuccess"] = "Email Updated Successfully";
        //        //    break;
        //    //    default:
        //    //        break;
        //    //}


        //    //return View();

        //}
        //POST: /User/Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Account(BaseViewModels model, HttpPostedFileBase file, string id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Login");
            switch (id)
            {
                case "profile":
                    ViewBag.Change = "ChangeMyProfile";
                    if (ChangeMyProfile(model.user))
                        TempData["profilesuccess"] = "Profile Updated Successfully";
                    break;
                case "password":
                    ViewBag.Change = "ChangeMyPassword";
                    if (ChangeMyPassword(model.user))
                        TempData["passwordsuccess"] = "Password Updated Successfully";
                    break;
                case "picture":
                    ViewBag.Change = "ChangeMyPicture";
                    if (file != null)
                    {
                        if (!HttpPostedFileBaseExtensions.IsImage(file))
                            ModelState.AddModelError("", "File is not an Image");
                        if (ChangeMyPicture(model.user, file))
                            TempData["picturesuccess"] = "Profile Picture Updated Successfully";
                    }
                    else
                        ModelState.AddModelError("", "Select File to upload");
                    break;
                case "email":
                    ViewBag.Change = "ChangeMyEmail";
                    if (db.users.FirstOrDefault(m => m.Email == model.user.Email) != null)
                        ModelState.AddModelError("user.Email", "Email Already used");
                    if (ChangeMyEmail(model.user))
                        TempData["emailsuccess"] = "Email Updated Successfully";
                    break;
                case "address":
                    ViewBag.Change = "ChangeMyAddress";
                    if (ChangeMyAddress(model.address))
                        TempData["addresssuccess"] = "Address Updated Successfully";
                    break;
                default:
                    break;
            }
            user user = Session["user"] as user;
            address address = db.addresses.FirstOrDefault(a => a.User_ID == user.ID);
            model.user = user;
            model.address = address;
            return View(model);
        }

        private bool ChangeMyProfile([Bind(Include = "user")] user user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Picture");
            ModelState.Remove("Type_id");
            ModelState.Remove("Active");
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                user.Token = _user.Token;
                user.Picture = _user.Picture;
                user.Password = _user.Password;
                user.ConfirmPassword = _user.Password;
                user.Type_id = _user.Type_id;
                user.Active = _user.Active;
                user.Email = _user.Email;
                db.Entry(_user).CurrentValues.SetValues(user);
                db.SaveChanges();
                Session["user"] = _user;
                return true;
            }
            return false;
        }

        private bool ChangeMyPassword([Bind(Include = "ID,Password,ConfirmPassword")] user user)
        {
            ModelState.Remove("Name");
            ModelState.Remove("Mobile");
            ModelState.Remove("Picture");
            ModelState.Remove("Type_id");
            ModelState.Remove("Active");;
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Password = user.Password;
                _user.ConfirmPassword = user.ConfirmPassword;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["user"] = _user;
                return true;
            }
            return false;
        }

        private bool ChangeMyEmail([Bind(Include = "ID,Email")] user user)
        {
            ModelState.Remove("Name");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Mobile");
            ModelState.Remove("Picture");
            ModelState.Remove("Type_id");
            ModelState.Remove("Active");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Email = user.Email;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["user"] = _user;
                if (Request.Cookies["user"] != null)
                {
                    HttpCookie cookie = Request.Cookies["user"];
                    cookie = new HttpCookie("user", _user.Email);
                    Response.SetCookie(cookie);
                }
                return true;
            }
            return false;
        }

        private bool ChangeMyPicture([Bind(Include = "ID")] user user, HttpPostedFileBase file)
        {
            ModelState.Remove("Name");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Mobile");
            ModelState.Remove("Picture");
            ModelState.Remove("Type_id");
            ModelState.Remove("Active");
            ModelState.Remove("Email");
            if (file != null)
            {
                string ext = Path.GetExtension(file.FileName);
                string filename = user.ID.ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/images/Profile/"), filename);
                // file is uploaded
                file.SaveAs(path);
                user _user = db.users.Find(user.ID);
                _user.Picture = "images/Profile/" + filename;
                db.Entry(_user).CurrentValues.SetValues(_user);
                user = _user;
                db.SaveChanges();
                Session["user"] = _user;
                return true;
            }
            return false;
        }

        private bool ChangeMyAddress([Bind(Include = "ID,Address1,City,Country,Zipcode")] address address)
        {
            if (ModelState.IsValid)
            {
                address _address = db.addresses.Find(address.ID);
                address.User_ID = _address.User_ID;
                db.Entry(_address).CurrentValues.SetValues(address);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        // GET: reports/Create
        public ActionResult AddComplain()
        {
            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name");
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type");
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name");
            return View();
        }

        // POST: reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComplain([Bind(Include = "ID,Product_ID,Description")] report report)
        {
            if (ModelState.IsValid)
            {
                user user = Session["user"] as user;
                report.User_ID = user.ID;
                report.Time = DateTime.Now;
                report.Type_ID = 1;
                db.reports.Add(report);
                db.SaveChanges();
                return RedirectToAction("Home","Home");
            }

            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name", report.Product_ID);
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type", report.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", report.User_ID);
            return View(report);
        }

        // GET: Admin/SalesPerson
        public ActionResult Orders(string search = "")
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "User");
            user user = Session["user"] as user;
            List<order> orders = new List<order>();
            if (search == "")
                orders = db.orders.Include(o => o.order_status).Include(o => o.payment_method1).Include(o => o.user).Where(o=>o.User_ID == user.ID).ToList();
            else
                orders = db.orders.Include(o => o.order_status).Include(o => o.payment_method1).Include(o => o.user).Where(o => o.User_ID == user.ID).Where(o => o.ID.ToString() == search || o.user.Name.Contains(search)).ToList();
            return View(orders);
        }

        public ActionResult Message()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "User");
            user user = Session["user"] as user;
            List<report> reports = db.reports.Where(r => r.User_ID == user.ID && r.Type_ID == 4).OrderByDescending(r => r.Time).ToList();
            return View(reports);
        }

        public ActionResult Viewmessage(int? id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "User");
            user user = Session["user"] as user;
            report report = db.reports.Find(id);
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
        }

        // GET: Admin/SalesPerson/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "User");
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //Edit Order Status
        // POST: Admin/SalesPerson/Edit/5
        public ActionResult cancel(int? id)
        {
            if (ModelState.IsValid)
            {
                order ord = db.orders.Find(id);
                ord.Status = 5;
                db.Entry(ord).CurrentValues.SetValues(ord);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Orders");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}