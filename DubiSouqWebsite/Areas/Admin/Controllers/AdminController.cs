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

namespace DubiSouqWebsite.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private Entities db = new Entities();

        // GET: /Admin/
        public ActionResult Index()
        {
            if (Request.Cookies["admin"] != null && Session["admin"] == null)
            {
                string email = Request.Cookies["admin"].Value;
                user us = db.users.Single(u => u.Email == email);
                Session["admin"] = us;
            }
            if (Session["admin"] == null)
                return RedirectToAction("Login", "User", new { Area = "" });
            else
                switch ((Session["admin"] as user).Type_id)
                {
                    case 1:
                        return RedirectToAction("index", "Error", new { error = " 403 Not Authorized Access", Area = "" });
                    case 2:
                        return RedirectToAction("index", "Manger");
                    case 3:
                        return RedirectToAction("Userindex", "Admin");
                    case 4:
                        return RedirectToAction("index", "SalesPerson");
                    case 5:
                        return RedirectToAction("index", "CustomerService");
                    case 6:
                        return RedirectToAction("index", "Inventory");
                    default:
                        return RedirectToAction("index", "Error", new { error = " 403 Not Authorized Access", Area = "" });
                }
        }

        // GET: /Admin/Admin/UserIndex
        public ActionResult UserIndex(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            List<user> users = new List<user>();
            if (search == "")
                users = db.users.Include(u => u.user_type).ToList();
            else
                users = db.users.Where(u => u.Name.Contains(search) || u.Email.Contains(search)).Include(u => u.user_type).ToList();

            return View(users);
        }

        // GET: /Admin/Admin/UserDetails/5
        public ActionResult UserDetails(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            address address = db.addresses.SingleOrDefault(ad => ad.User_ID == user.ID);
            BaseViewModels model = new BaseViewModels();
            model.address = address;
            model.user = user;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: /Admin/Admin/AddUser
        public ActionResult AddUser()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name");
            return View();
        }

        // POST: /Admin/Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser([Bind(Include = "ID,Name,Email,Mobile,Password,ConfirmPassword,Active,Type_id")] user user)
        {
            ModelState.Remove("Picture");
            if (ModelState.IsValid)
            {
                user.Token = null;
                user.Picture = "images/Profile/default.png";
                db.users.Add(user);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 11, db.users.Max(u => u.ID), user.Email);
                return RedirectToAction("Index");
            }
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name", user.Type_id);
            return View(user);
        }

        // GET: /Admin/Admin/EditUser/5
        public ActionResult EditUser(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name", user.Type_id);
            return View(user);
        }

        // POST: /Admin/Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser([Bind(Include = "ID,Name,Mobile,Active,Type_id")] user user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Picture");
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                user.Token = _user.Token;
                user.Picture = _user.Picture;
                user.Password = _user.Password;
                user.ConfirmPassword = _user.Password;
                user.Email = _user.Email;
                db.Entry(_user).CurrentValues.SetValues(user);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, user.ID, user.Email);
                return RedirectToAction("EditUser", new { id = user.ID });
            }
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name", user.Type_id);
            return View(user);
        }

        // GET: /Admin/Admin/ChangePassword/5
        public ActionResult ChangePassword(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Admin/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "ID,Password,ConfirmPassword")] user user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Password = user.Password;
                _user.ConfirmPassword = user.ConfirmPassword;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _user.ID, _user.Email);
                return RedirectToAction("EditUser", new { id = user.ID });
            }
            return View(user);
        }

        // GET: /Admin/Admin/ChangeEmail/5
        public ActionResult ChangeEmail(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Admin/ChangeEmail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail([Bind(Include = "ID,Email")] user user)
        {
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Email = user.Email;
                _user.ConfirmPassword = _user.Password;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _user.ID, _user.Email);
                return RedirectToAction("EditUser", new { id = user.ID });
            }
            return View(user);
        }

        // GET: /Admin/Admin/ChangePicture/5
        public ActionResult ChangePicture(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Admin/ChangePicture/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePicture([Bind(Include = "ID")] user user, HttpPostedFileBase file)
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
                if (!HttpPostedFileBaseExtensions.IsImage(file))
                {
                    ModelState.AddModelError("", "File is not an Image");
                    return View(user);
                }
                string ext = Path.GetExtension(file.FileName);
                string filename = user.ID.ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/images/Profile/"), filename);
                // file is uploaded
                file.SaveAs(path);
                user us = db.users.Find(user.ID);
                us.Picture = "images/Profile/" + filename;
                db.Entry(us).CurrentValues.SetValues(us);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, us.ID, us.Email);
                return RedirectToAction("ChangePicture", new { id = user.ID });
            }
            return View(user);
        }

        // GET: /Admin/Admin/ChangeAddress/5
        public ActionResult ChangeAddress(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            address address = db.addresses.SingleOrDefault(u => u.User_ID == user.ID);
            return View(address);
        }

        // POST: /Admin/Admin/ChangeAddress/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAddress(int id,[Bind(Include = "ID,Address1,City,Country,Zipcode")] address address)
        {
            if (ModelState.IsValid)
            {
                address _address = db.addresses.Include(u => u.user).SingleOrDefault(d => d.User_ID == id);
                address.User_ID = _address.User_ID;
                address.ID = _address.ID;
                db.Entry(_address).CurrentValues.SetValues(address);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _address.User_ID, _address.user.Email);
                return RedirectToAction("EditUser", new { id = _address.User_ID });
            }
            return View(address);
        }

        // GET: /Admin/Inventory/DeleteUser/5
        public ActionResult DeleteUser(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Inventory/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedUser(int id)
        {
            user user = db.users.SingleOrDefault(u => u.ID == id);
            db.users.Remove(user);
            db.SaveChanges();
            ReportModel.CreateAdminReport((Session["admin"] as user).ID, 13, user.ID, user.Email);
            return RedirectToAction("Index");
        }

        // GET: /Admin/CustomerService/ViewReport
        public ActionResult ViewReport(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index", "Admin");
            List<report> reports = new List<report>();
            if (search == "")
                reports = db.reports.Include(r => r.user).Where(r => r.Type_ID == 2).OrderByDescending(r => r.Time).ToList();
            else
                reports = db.reports.Include(r => r.user).Where(r => r.Type_ID == 2).Where(u => u.Description.Contains(search) || u.user.Name.Contains(search)).OrderByDescending(r => r.Time).ToList();
            return View(reports);
        }

        // GET: /Admin/CustomerService/ReportDetails/5
        public ActionResult ReportDetails(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 3)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            report report = db.reports.Include(r => r.user).Include(r => r.product).Include(r => r.product.category).SingleOrDefault(r => r.ID == id && r.Type_ID == 2);
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
        }

        // GET: /Admin/Admin/EditMyUser
        public ActionResult EditMyUser()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id == 1)
                return RedirectToAction("index");
            user user = Session["admin"] as user;
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name", user.Type_id);
            return View(user);
        }

        // POST: /Admin/Admin/EditMyUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMyUser([Bind(Include = "ID,Name,Mobile")] user user)
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
                _user.Name = user.Name;
                _user.ConfirmPassword = _user.Password;
                _user.Mobile = user.Mobile;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["admin"] = _user;
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, user.ID, user.Email);
                return RedirectToAction("EditMyUser");
            }
            ViewBag.Type_id = new SelectList(db.user_type, "ID", "Name", user.Type_id);
            return View(user);
        }

        // GET: /Admin/Admin/ChangeMyPassword
        public ActionResult ChangeMyPassword()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id == 1)
                return RedirectToAction("index");
            user user = Session["admin"] as user;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Admin/ChangeMyPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyPassword([Bind(Include = "ID,Password,ConfirmPassword")] user user)
        {
            ModelState.Remove("Name");
            ModelState.Remove("Mobile");
            ModelState.Remove("Picture");
            ModelState.Remove("Type_id");
            ModelState.Remove("Active");
            ModelState.Remove("Email");
            if (ModelState.IsValid)
            {
                user _user = db.users.Find(user.ID);
                _user.Password = user.Password;
                _user.ConfirmPassword = user.ConfirmPassword;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["admin"] = _user;
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _user.ID, _user.Email);
                return RedirectToAction("EditMyUser");
            }
            return View(user);
        }

        // GET: /Admin/Admin/ChangeMyEmail
        public ActionResult ChangeMyEmail()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id == 1)
                return RedirectToAction("index");
            user user = Session["admin"] as user;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Admin/ChangeMyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyEmail([Bind(Include = "ID,Email")] user user)
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
                _user.ConfirmPassword = _user.Password;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["admin"] = _user;
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _user.ID, _user.Email);
                return RedirectToAction("EditMyUser");
            }
            return View(user);
        }

        // GET: Admin/Admin/ChangeMyPicture
        public ActionResult ChangeMyPicture()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id == 1)
                return RedirectToAction("index");
            user user = Session["admin"] as user;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Admin/Admin/ChangeMyPicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMyPicture([Bind(Include = "ID")] user user, HttpPostedFileBase file)
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
                if (!HttpPostedFileBaseExtensions.IsImage(file))
                {
                    ModelState.AddModelError("", "File is not an Image");
                    return View(user);
                }
                string ext = Path.GetExtension(file.FileName);
                string filename = user.ID.ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/images/Profile/"), filename);
                // file is uploaded
                file.SaveAs(path);
                user _user = db.users.Find(user.ID);
                _user.Picture = "images/Profile/" + filename;
                _user.ConfirmPassword = _user.Password;
                db.Entry(_user).State = EntityState.Modified;
                db.SaveChanges();
                Session["admin"] = _user;
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _user.ID, _user.Email);
                return RedirectToAction("ChangeMyPicture");
            }
            return View(user);
        }

        // GET: /Admin/Admin/ChangeMyAddress
        public ActionResult ChangeMyAddress()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id == 1)
                return RedirectToAction("index");
            user user = Session["admin"] as user;
            if (user == null)
            {
                return HttpNotFound();
            }
            address address = db.addresses.SingleOrDefault(u => u.User_ID == user.ID);
            return View(address);
        }

        // POST: /Admin/Admin/ChangeMyAddress
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
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, _address.User_ID, _address.user.Email);
                return RedirectToAction("EditMyUser");
            }
            return View(address);
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