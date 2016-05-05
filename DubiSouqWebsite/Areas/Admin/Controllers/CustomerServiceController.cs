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
    public class CustomerServiceController : Controller
    {
        private Entities db = new Entities();

        // GET: Admin/CustomerService
        public ActionResult Index(string search ="")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            List<user> users = new List<user>();
            if (search == "")
                users = db.users.Where(u => u.Type_id == 1).ToList();
            else
                users = db.users.Where(u => u.Type_id == 1).Where(u => u.Name.Contains(search) || u.Email.Contains(search)).ToList();
            return View(users);
        }

        // GET: Admin/CustomerService/UserIndex
        public ActionResult UserIndex()
        {
            return RedirectToAction("Index");
        }

        // GET: Admin/CustomerService/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            address address = db.addresses.SingleOrDefault(u => u.User_ID == id);
            BaseViewModels model = new BaseViewModels();
            model.address = address;
            model.user = user;
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: Admin/CustomerService/EditUser/5
        public ActionResult EditUser(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Error", new { error = " 403 Not Authorized Access", Area = "" });
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/CustomerService/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser([Bind(Include = "ID,Name,Mobile,Active")] user user)
        {
            if (ModelState.IsValid)
            {
                user us = db.users.Find(user.ID);
                user.Type_id = us.Type_id;
                user.Token = us.Token;
                user.Email = us.Email;
                user.Password = us.Password;
                user.ConfirmPassword = us.ConfirmPassword;
                user.Picture = us.Picture;
                db.Entry(us).CurrentValues.SetValues(user);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, us.ID, us.Email);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/CustomerService/ChangePassword/5
        public ActionResult ChangePassword(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/CustomerService/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "ID,Password,ConfirmPassword")] user user)
        {
            if (ModelState.IsValid)
            {
                user us = db.users.Find(user.ID);
                us.Password = user.Password;
                us.ConfirmPassword = us.ConfirmPassword;
                db.Entry(us).CurrentValues.SetValues(us);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, us.ID, us.Email);
                return RedirectToAction("EditUser", new { id = user.ID });
            }
            return View(user);
        }

        // GET: Admin/CustomerService/ChangeEmail/5
        public ActionResult ChangeEmail(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/CustomerService/ChangeEmail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail([Bind(Include = "ID,Email")] user user)
        {
            if (ModelState.IsValid)
            {
                if (db.users.SingleOrDefault(u => u.Email == user.Email) != null)
                {
                    ModelState.AddModelError("", user.Email + " is Already Used");
                    return View(user);
                }
                user us = db.users.Find(user.ID);
                us.Email = user.Email;
                db.Entry(us).CurrentValues.SetValues(us);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, us.ID, us.Email);
                return RedirectToAction("EditUser", new { id=user.ID });
            }
            return View(user);
        }

        // GET: Admin/CustomerService/ChangePicture/5
        public ActionResult ChangePicture(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/CustomerService/ChangePicture/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePicture([Bind(Include = "ID")] user user, HttpPostedFileBase file)
        {
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
                us.Picture= "images/Profile/" + filename;
                db.Entry(us).CurrentValues.SetValues(us);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, us.ID, us.Email);
                return RedirectToAction("ChangePicture", new { id = user.ID });
            }
            return View(user);
        }

        // GET: Admin/CustomerService/ChangeAddress/5
        public ActionResult ChangeAddress(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.SingleOrDefault(u => u.ID == id && u.Type_id == 1);
            if (user == null)
            {
                return HttpNotFound();
            }
            address address = db.addresses.SingleOrDefault(u => u.User_ID == user.ID);
            return View(address);
        }

        // POST: Admin/CustomerService/ChangeAddress/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAddress([Bind(Include = "ID,Address1,City,Country,Zipcode")] address address)
        {
            if (ModelState.IsValid)
            {
                address ad = db.addresses.Find(address.ID);
                address.User_ID = ad.User_ID;
                db.Entry(ad).CurrentValues.SetValues(address);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 12, ad.User_ID, ad.user.Email);
                return RedirectToAction("EditUser", new { id = ad.User_ID });
            }
            return View(address);
        }

        // GET: Admin/CustomerService/ViewReport
        public ActionResult ViewReport(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            List<report> reports = new List<report>();
            if (search == "")
                reports = db.reports.Include(r => r.user).Where(r=>r.Type_ID == 1).Where(u => u.Description.Contains(search) || u.user.Name.Contains(search)).OrderByDescending(r => r.Time).ToList();
            return View(reports);
        }

        // GET: Admin/CustomerService/ReportDetails/5
        public ActionResult ReportDetails(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 5)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            report report = db.reports.Include(r => r.user).Include(r => r.product).Include(r => r.product.category).SingleOrDefault(r => r.ID == id && r.Type_ID == 1 );
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
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