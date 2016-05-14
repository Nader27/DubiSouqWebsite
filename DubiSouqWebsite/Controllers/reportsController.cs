using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DubiSouqWebsite.Models;

namespace DubiSouqWebsite.Controllers
{
    public class reportsController : Controller
    {
        private Entities db = new Entities();

        // GET: reports
        public ActionResult Index()
        {
            var reports = db.reports.Include(r => r.product).Include(r => r.report_Type).Include(r => r.user);
            return View(reports.ToList());
        }

        // GET: reports/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            report report = db.reports.Find(id);
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
        }

        // GET: reports/Create
        public ActionResult Create()
        {
            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name");
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type");
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name");
            return View();
        }

        // POST: reports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,User_ID,Product_ID,Description,Type_ID,Time")] report report)
        {
            if (ModelState.IsValid)
            {
                db.reports.Add(report);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name", report.Product_ID);
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type", report.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", report.User_ID);
            return View(report);
        }

        // GET: reports/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            report report = db.reports.Find(id);
            if (report == null)
            {
                return HttpNotFound();
            }
            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name", report.Product_ID);
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type", report.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", report.User_ID);
            return View(report);
        }

        // POST: reports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,User_ID,Product_ID,Description,Type_ID,Time")] report report)
        {
            if (ModelState.IsValid)
            {
                db.Entry(report).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Product_ID = new SelectList(db.products, "ID", "Name", report.Product_ID);
            ViewBag.Type_ID = new SelectList(db.report_Type, "ID", "Type", report.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", report.User_ID);
            return View(report);
        }

        // GET: reports/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            report report = db.reports.Find(id);
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
        }

        // POST: reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            report report = db.reports.Find(id);
            db.reports.Remove(report);
            db.SaveChanges();
            return RedirectToAction("Index");
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
