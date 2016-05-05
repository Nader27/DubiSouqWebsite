using DubiSouqWebsite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DubiSouqWebsite.Controllers
{
    public class UserController : Controller
    {

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /User/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Session["user"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        // GET: /User/Login
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Session["user"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpPost]
        public ActionResult Login(BaseViewModels model)
        {
            Entities db = new Entities();
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
            return View(model);
        }

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
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        //POST: /User/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(BaseViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Entities db = new Entities();
                    if (db.users.FirstOrDefault(m => m.Email == model.user.Email) == null)
                    {
                        model.user.Active = true;
                        model.user.Type_id = 1;
                        db.users.Add(model.user);
                        db.SaveChanges();
                        model.address.User_ID = db.users.Max(m => m.ID);
                        db.addresses.Add(model.address);
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("user.Email", "Email Already used");
                        return View(model);
                    }
                }
                catch
                {
                    return View(model);
                }
                if (ViewBag.ReturnUrl != null)
                    return RedirectToRoute(ViewBag.ReturnUrl);
                else return RedirectToAction("Home", "Home");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult UploadProfileImage()
        {
            try
            {
                if (Session["user"] == null)
                    return RedirectToAction("login");
                user sess = Session["user"] as user;
                if (sess.Type_id != 1)
                    return RedirectToAction("editprofile", "Admin", new { id = (Session["user"] as user).ID });
                Entities db = new Entities();
                user us = db.users.Single(use => use.ID == sess.ID);
                return View(us);
            }
            catch
            {
                return RedirectToAction("index", "Error", new { error = "Edit Failed" });
            }
        }

        [HttpPost]
        public ActionResult UploadProfileImage(int id, HttpPostedFileBase file)
        {
            if (file != null)
            {
                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/images/profile"), pic);
                // file is uploaded
                file.SaveAs(path);
                Entities db = new Entities();
                user us = db.users.Single(use => use.ID == id);
                us.Picture = "images/profile/" + pic;
                us.ConfirmPassword = us.Password;
                db.Entry(us).CurrentValues.SetValues(us);
                db.SaveChanges();
                // save the image path path to the database or you can send image 
                // directly to database
                // in-case if you want to store byte[] ie. for DB
                //using (MemoryStream ms = new MemoryStream())
                //{
                //   file.InputStream.CopyTo(ms);
                //   byte[] array = ms.GetBuffer();
                //}

            }
            // after successfully uploading redirect the user
            return RedirectToAction("editprofile", "Admin", new { id = id });
        }

        [HttpGet]
        public ActionResult Edit()
        {
            try
            {
                if (Session["user"] == null)
                    return RedirectToAction("login");
                user sess = Session["user"] as user;
                if (sess.Type_id != 1)
                    return RedirectToAction("editprofile", "Admin", new { id = (Session["user"] as user).ID });
                Entities db = new Entities();
                user us = db.users.Single(use => use.ID == sess.ID);
                address add = db.addresses.Single(use => use.User_ID == sess.ID);
                BaseViewModels bvm = new BaseViewModels();
                bvm.address = add;
                bvm.user = us;
                return View(bvm);
            }
            catch
            {
                return RedirectToAction("index", "Error", new { error = "Edit Failed" });
            }
        }

        [HttpPost]
        public ActionResult Edit(BaseViewModels model)
        {
            try
            {
                Entities db = new Entities();
                user us = db.users.Single(use => use.ID == model.user.ID);
                model.user.ID = us.ID;
                model.user.Email = us.Email;
                model.user.Password = us.Password;
                model.user.ConfirmPassword = us.Password;
                model.user.Picture = us.Picture;
                model.user.Type_id = us.Type_id;
                model.user.Active = us.Active;
                model.user.Token = us.Token;
                db.Entry(us).CurrentValues.SetValues(model.user);
                db.SaveChanges();
                address add = db.addresses.Single(use => use.User_ID == model.user.ID);
                db.Entry(add).CurrentValues.SetValues(model.address);
                db.SaveChanges();
                return RedirectToAction("Home", "Home");
            }
            catch
            {
                return View(model);
            }
        }

    }
}