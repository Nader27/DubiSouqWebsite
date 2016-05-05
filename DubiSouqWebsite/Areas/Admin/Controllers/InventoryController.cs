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
    public class InventoryController : Controller
    {
        private Entities db = new Entities();

        // GET: Admin/Inventory
        public ActionResult Index(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
            List<product> products = new List<product>();
            if (search == "")
                products = db.products.Include(p => p.category).Where(u => u.Type_ID == 1).ToList();
            else
                products = db.products.Include(p => p.category).Where(u => u.Type_ID == 1).Where(u => u.Name.Contains(search) || u.category.Name.Contains(search)).ToList();
            return View(products);
        }

        // GET: Admin/Inventory/ProductsIndex
        public ActionResult ProductsIndex()
        {
            return RedirectToAction("Index");
        }

        // GET: Admin/Inventory/AddProduct
        public ActionResult AddProduct()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name");
            return View();
        }

        // POST: Admin/Inventory/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct([Bind(Include = "ID,Name,Category_ID,Price,Description,Quantity")] product product)
        {
            if (ModelState.IsValid)
            {
                user USER = Session["admin"] as user;
                product.Post_Time = DateTime.Now;
                product.User_ID = USER.ID;
                product.Sale = 0;
                product.Type_ID = 1;
                db.products.Add(product);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 21, db.products.Max(p => p.ID), product.Name);
                return RedirectToAction("Index");
            }
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name", product.Category_ID);
            return View(product);
        }

        // GET: Admin/Inventory/EditProduct/5
        public ActionResult EditProduct(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
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
            return View(product);
        }

        // POST: Admin/Inventory/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct([Bind(Include = "ID,Name,Category_ID,Price,Description,Quantity")] product product)
        {
            if (ModelState.IsValid)
            {
                product prod = db.products.SingleOrDefault(u => u.ID == product.ID && u.Type_ID == 1);
                if (prod == null)
                {
                    return HttpNotFound();
                }
                prod.Name = product.Name;
                prod.Category_ID = product.Category_ID;
                prod.Price = product.Price;
                prod.Description = product.Description;
                prod.Quantity = product.Quantity;
                db.Entry(prod).CurrentValues.SetValues(prod);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 22, prod.ID, prod.Name);
                return RedirectToAction("Index");
            }
            ViewBag.Category_ID = new SelectList(db.categories, "ID", "Name", product.Category_ID);
            return View(product);
        }

        // GET: Admin/Inventory/DeleteProduct/5
        public ActionResult DeleteProduct(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.SingleOrDefault(u => u.ID == id && u.Type_ID == 1);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Inventory/DeleteProduct/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedProduct(int id)
        {
            product product = db.products.SingleOrDefault(u => u.ID == id && u.Type_ID == 1);
            ReportModel.CreateAdminReport((Session["admin"] as user).ID, 23, product.ID, product.Name);
            db.products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Inventory/ProductImage/5
        [HttpGet]
        public ActionResult ProductImage(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.SingleOrDefault(p => p.ID == id && p.Type_ID == 1);
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
                product product = db.products.SingleOrDefault(p => p.ID == id && p.Type_ID == 1);
                if (product == null)
                {
                    return HttpNotFound();
                }
                if (!HttpPostedFileBaseExtensions.IsImage(file))
                {
                    ModelState.AddModelError("", "File is not an Image");
                    return RedirectToAction("ProductImage", new { id = id });
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

        // GET: Admin/Inventory/DeleteProductImage/5?img=5
        [HttpGet]
        public ActionResult DeleteProductImage(int id, int img)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 6)
                return RedirectToAction("index", "Admin");
            product_picture product_picture = db.product_picture.Find(img);
            if (product_picture.Product_ID != id)
            {
                return HttpNotFound();
            }
            product product = db.products.SingleOrDefault(p => p.ID == product_picture.Product_ID && p.Type_ID == 1);
            if (product == null)
            {
                return HttpNotFound();
            }
            db.product_picture.Remove(product_picture);
            db.SaveChanges();
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