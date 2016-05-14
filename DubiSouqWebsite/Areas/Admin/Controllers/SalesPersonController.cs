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
    public class SalesPersonController : Controller
    {
        private Entities db = new Entities();

        // GET: Admin/SalesPerson
        public ActionResult Index(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            List<order> orders = new List<order>();
            if (search == "")
                orders = db.orders.Include(o => o.order_status).Include(o => o.payment_method1).Include(o => o.user).ToList();
            else
                orders = db.orders.Include(o => o.order_status).Include(o => o.payment_method1).Include(o => o.user).Where(o => o.ID.ToString() == search || o.user.Name.Contains(search)).ToList();
            return View(orders);
        }

        // GET: Admin/SalesPerson/ProductsIndex
        public ActionResult ProductsIndex(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            List<product> products = new List<product>();
            if (search == "")
                 products = db.products.Include(p => p.category).Where(u => u.Type_ID == 1).ToList();
            else
                products = db.products.Include(p => p.category).Where(u => u.Type_ID == 1).Where(u => u.Name.Contains(search) || u.category.Name.Contains(search)).ToList();
            return View(products);
        }

        // GET: Admin/SalesPerson/ProductsIndex
        public ActionResult OffersIndex(string search = "")
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            List<product> products = new List<product>();
            if (search == "")
                products = db.products.Include(p => p.category).Where(u => u.Type_ID == 3 || u.Type_ID == 2).ToList();
            else
                products = db.products.Include(p => p.category).Where(u => u.Type_ID == 3 || u.Type_ID == 2).Where(u => u.Name.Contains(search) || u.category.Name.Contains(search)).ToList();
            return View(products);
        }

        // GET: Admin/SalesPerson/OrderIndex
        public ActionResult OrderIndex()
        {
            return RedirectToAction("Index");
        }

        // GET: Admin/SalesPerson/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //Edit Order Status
        // GET: Admin/SalesPerson/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(db.order_status, "ID", "Status", order.Status);
            return View(order);
        }

        //Edit Order Status
        // POST: Admin/SalesPerson/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Status")] order order)
        {
            if (ModelState.IsValid)
            {
                order ord = db.orders.Find(order.ID);
                ord.Status = order.Status;
                db.Entry(ord).CurrentValues.SetValues(ord);
                db.SaveChanges();
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 4, ord.ID, ord.order_status.Status);
                return RedirectToAction("Index");
            }
            ViewBag.Status = new SelectList(db.order_status, "ID", "Status", order.Status);
            return View(order);
        }

        //Edit Sale
        // GET: Admin/Inventory/EditProduct/5
        public ActionResult EditProduct(int? id)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
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
            return View(product);
        }

        //Edit Sale
        // POST: Admin/Inventory/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct([Bind(Include = "ID,Sale")] product product)
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 4)
                return RedirectToAction("index", "Admin");
            ModelState.Remove("Name");
            ModelState.Remove("Description");
            ModelState.Remove("Post_Time");
            ModelState.Remove("Quantity");
            ModelState.Remove("Type_ID");
            ModelState.Remove("User_ID");
            ModelState.Remove("Price");
            ModelState.Remove("Category_ID");
            if (ModelState.IsValid)
            {
                product prod = db.products.SingleOrDefault(u => u.ID == product.ID && u.Type_ID == 1);
                if (prod == null)
                {
                    return HttpNotFound();
                }
                prod.Sale = product.Sale;
                db.Entry(prod).CurrentValues.SetValues(prod);
                ReportModel.CreateAdminReport((Session["admin"] as user).ID, 3, prod.ID, prod.Sale.ToString());
                db.SaveChanges();
                return RedirectToAction("ProductsIndex");
            }
            return View(product);
        }

        public ActionResult ChangeStatus(int? id)
        {
            product _product = db.products.Find(id);
            if(_product.Type_ID == 2)
                _product.Type_ID = 3;
            else
                _product.Type_ID = 2;
            db.Entry(_product).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("OffersIndex");
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
