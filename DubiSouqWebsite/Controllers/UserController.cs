using DubiSouqWebsite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;

namespace DubiSouqWebsite.Controllers
{
    public class UserController : Controller
    {
        private Entities db = new Entities();

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

        //GET: /User/Account
        public ActionResult Account()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login");
            return View();
        }

        // GET: /User/Login
        [AllowAnonymous]
        public ActionResult Login()
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

        //POST: /User/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(BaseViewModels model)
        {
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
            if (ModelState.IsValid)
            {
                if (db.users.FirstOrDefault(m => m.Email == model.user.Email) == null)
                {
                    model.user.Active = true;
                    model.user.Type_id = 1;
                    db.users.Add(model.user);
                    db.SaveChanges();
                    model.address.User_ID = db.users.Max(m => m.ID);
                    db.addresses.Add(model.address);
                    db.SaveChanges();
                    int ID = db.users.Max(u => u.ID);
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

        // GET: /User/ChangeMyProfile
        public PartialViewResult ChangeMyProfile()
        {
            user user = Session["user"] as user;
            return PartialView(user);
        }

        // POST: /User/ChangeMyProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyProfile([Bind(Include = "ID,Name,Mobile")] user user)
        {
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
                TempData["success"] = "Your Balance is now zero";
                return RedirectToAction("Account");
            }
            return new EmptyResult();
        }

        // GET: /User/ChangeMyPassword
        public PartialViewResult ChangeMyPassword()
        {

            user user = Session["user"] as user;
            return PartialView(user);
        }

        // POST: /User/ChangeMyPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyPassword([Bind(Include = "ID,Password,ConfirmPassword")] user user)
        {
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Password = user.Password;
                _user.ConfirmPassword = user.ConfirmPassword;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["user"] = _user;
                return RedirectToAction("Account");
            }
            return new EmptyResult();
        }

        // GET: /User/ChangeMyEmail
        public PartialViewResult ChangeMyEmail()
        {
            user user = Session["user"] as user;
            return PartialView(user);
        }

        // POST: /User/ChangeMyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyEmail([Bind(Include = "ID,Email")] user user)
        {
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Email = user.Email;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["user"] = _user;
                return RedirectToAction("Account");
            }
            return new EmptyResult();
        }

        // GET: /User/ChangeMyPicture
        public PartialViewResult ChangeMyPicture()
        {
            user user = Session["user"] as user;
            return PartialView(user);
        }

        // POST: /User/ChangeMyPicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyPicture([Bind(Include = "ID")] user user, HttpPostedFileBase file)
        {
            if (file != null)
            {
                if (!HttpPostedFileBaseExtensions.IsImage(file))
                {
                    ModelState.AddModelError("", "File is not an Image");
                    return PartialView(user);
                }
                string ext = Path.GetExtension(file.FileName);
                string filename = user.ID.ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/images/Profile/"), filename);
                // file is uploaded
                file.SaveAs(path);
                user _user = db.users.Find(user.ID);
                _user.Picture = "images/Profile/" + filename;
                db.Entry(_user).CurrentValues.SetValues(_user);
                db.SaveChanges();
                Session["user"] = _user;
                return RedirectToAction("Account");
            }
            return new EmptyResult();
        }

        // GET: /User/ChangeMyAddress
        public PartialViewResult ChangeMyAddress()
        {
            user user = Session["user"] as user;
            address address = db.addresses.SingleOrDefault(u => u.User_ID == user.ID);
            return PartialView(address);
        }

        // POST: /User/ChangeMyAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyAddress([Bind(Include = "ID,Address1,City,Country,Zipcode")] address address)
        {
            if (ModelState.IsValid)
            {
                address _address = db.addresses.Find(address.ID);
                address.User_ID = _address.User_ID;
                db.Entry(_address).CurrentValues.SetValues(address);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["user"] as user).ID, 12, _address.User_ID, _address.user.Email);
                return RedirectToAction("Account");
            }
            return new EmptyResult();
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