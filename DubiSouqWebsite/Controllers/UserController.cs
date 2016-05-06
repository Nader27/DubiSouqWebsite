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
                    model.user.Picture = "images/Profile/default.png";
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