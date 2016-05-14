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

namespace DubiSouqWebsite.Controllers
{
    public class productsController : Controller
    {
        private Entities db = new Entities();

        // GET: products
        public ActionResult Index()
        {
            var products = db.products.Include(p => p.category).Include(p => p.product_type).Include(p => p.user).Where(p=>p.Type_ID == 2);
            return View(products.ToList());
        }

        // GET: products/Create
        public ActionResult Create()
        {
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name");
            ViewBag.Type_ID = new SelectList(db.product_type, "ID", "Name");
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name");
            return View();
        }

        // POST: products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Category_ID,Price,Description,Quantity")] product product)
        {
            user USER = Session["user"] as user;
            ModelState.Remove("Post_Time");
            ModelState.Remove("Sale");
            ModelState.Remove("Type_ID");
            ModelState.Remove("User_ID");
            if (ModelState.IsValid)
            {
                product.Post_Time = DateTime.Now;
                product.Sale = 0;
                product.Type_ID = 3;
                product.User_ID = USER.ID;
                db.products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name", product.Category_ID);
            ViewBag.Type_ID = new SelectList(db.product_type, "ID", "Name", product.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", product.User_ID);
            return View(product);
        }

        // GET: products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name", product.Category_ID);
            ViewBag.Type_ID = new SelectList(db.product_type, "ID", "Name", product.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", product.User_ID);
            return View(product);
        }

        // POST: products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Category_ID,Price,Description,Quantity")] product product)
        {
            ModelState.Remove("Post_Time");
            ModelState.Remove("Sale");
            ModelState.Remove("Type_ID");
            ModelState.Remove("User_ID");
            if (ModelState.IsValid)
            {
                product _product = db.products.Find(product.ID);
                _product.Quantity = product.Quantity;
                _product.Description = product.Description;
                _product.Price = product.Price;
                _product.Category_ID = product.Category_ID;
                _product.Name = product.Name; 
                db.Entry(_product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name", product.Category_ID);
            ViewBag.Type_ID = new SelectList(db.product_type, "ID", "Name", product.Type_ID);
            ViewBag.User_ID = new SelectList(db.users, "ID", "Name", product.User_ID);
            return View(product);
        }

        // GET: products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            product product = db.products.Find(id);
            db.products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Inventory/ProductImage/5
        [HttpGet]
        public ActionResult ProductImage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.SingleOrDefault(p => p.ID == id && p.Type_ID == 2);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.product_pictures = db.product_picture.Where(p => p.Product_ID == product.ID).ToList();
            return View();
        }

        // POST: Admin/Inventory/ProductImage/5
        [HttpPost, ActionName("ProductImage")]
        [ValidateAntiForgeryToken]
        public ActionResult UploadProductImage(int id, HttpPostedFileBase file)
        {
            if (file != null)
            {
                product product = db.products.SingleOrDefault(p => p.ID == id && p.Type_ID == 2);
                if (product == null)
                {
                    return HttpNotFound();
                }
                if (!HttpPostedFileBaseExtensions.IsImage(file))
                {
                    ModelState.AddModelError("", "File is not an Image");
                    return View();
                }
                string ext = Path.GetExtension(file.FileName);
                string filename = id.ToString() + ext;
                int count = 0;
                while (db.product_picture.FirstOrDefault(u => u.Picture == "images/Product/" + filename) != null)
                    filename = id.ToString() + '_' + (++count).ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/images/Product/"), filename);
                // file is uploaded
                file.SaveAs(path);
                product_picture product_picture = new product_picture();
                product_picture.Picture = "images/Product/" + filename;
                product_picture.Product_ID = id;
                db.product_picture.Add(product_picture);
                db.SaveChanges();
            }
            return RedirectToAction("ProductImage", new { id = id });
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
