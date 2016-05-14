using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DubiSouqWebsite.Models;
using System.IO;

namespace DubiSouqWebsite.Controllers
{
    public class OffersController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Offers(int? id, string search = "", int page = 0, string sort = "abc", string amount = "")
        {
            Entities db = new Entities();
            List<product> products = new List<product>();
            if (id == null)
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 2).ToList();
            else
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 2).Where(p => p.Category_ID == id).ToList();
            if (search != "")
                products = products.Where(p => p.Name.Contains(search) || p.category.Name.Contains(search) || p.Description.Contains(search)).ToList();
            if (amount != "")
            {
                string[] s = amount.Split('-');
                int start = int.Parse(s[0]);
                int end = int.Parse(s[1]);
                products = products.Where(p => p.Price >= start && p.Price <= end).ToList();
            }
            switch (sort)
            {
                case "abc":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                case "old":
                    products = products.OrderBy(p => p.Post_Time).ToList();
                    break;
                case "new":
                    products = products.OrderByDescending(p => p.Post_Time).ToList();
                    break;
                case "sale":
                    products = products.OrderByDescending(p => p.Sale).ToList();
                    break;
                case "Cheap":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "exp":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    break;
            }
            ////////////pagination////////
            const int PageSize = 12;
            int count = products.Count();
            products = products.Skip(page * PageSize).Take(PageSize).ToList();
            ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);
            ViewBag.productscount = count;
            ViewBag.size = PageSize;
            ViewBag.page = page;
            //////////////////////////////
            category categoory = db.categories.SingleOrDefault(u => u.ID == id);
            List<category> categories = db.categories.Where(u => u.Parent_Category == categoory.Parent_Category).ToList();
            if (categoory.category2 != null)
                ViewBag.Parent_Category = categoory.category2.Name;
            else
                ViewBag.Parent_Category = categoory.Name;
            ViewBag.category = categoory;
            ViewBag.categories = categories;
            return View(products);
        }

        //GET: /Shop/Product_Details/5
        public ActionResult Product_Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.Include(p => p.product_picture).Include(p => p.reviews).SingleOrDefault(p => p.ID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            int count = 0;
            int total = 0;
            foreach (review item in product.reviews)
            {
                if (item.Rate > 0)
                {
                    count++;
                    total += item.Rate;
                }
            }
            if (count != 0)
                ViewBag.Rate = total / count;
            else
                ViewBag.Rate = 0;
            if (Session["user"] == null)
                ViewBag.UserRate = 0;
            else
            {
                user USER = Session["user"] as user;
                review review = db.reviews.SingleOrDefault(r => r.User_ID == USER.ID && r.Product_ID == product.ID);
                if (review == null)
                    ViewBag.UserRate = 0;
                else
                    ViewBag.UserRate = review.Rate;
            }
            return View(product);
        }

        public ActionResult Review(int? id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.SingleOrDefault(p => p.ID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            user USER = Session["user"] as user;
            review review = db.reviews.SingleOrDefault(r => r.User_ID == USER.ID && r.Product_ID == product.ID);
            if (review != null)
            {
                review.Title = Request["title"];
                review.Comment = Request["comment"];
                review.Rate = int.Parse(Request["rate"]);
                review.Date = DateTime.Now;
                db.Entry(review).State = EntityState.Modified;
            }
            else
            {
                review = new review();
                review.User_ID = USER.ID;
                review.Product_ID = id.Value;
                review.Title = Request["title"];
                review.Comment = Request["comment"];
                review.Date = DateTime.Now;
                review.Rate = int.Parse(Request["rate"]);
                db.reviews.Add(review);
            }
            db.SaveChanges();
            return RedirectToAction("Product_Details", new { id = id });
        }

        public bool MakeOffer([Bind(Include = "ID,Name,Price,Description,Quantity,Category_ID")]product product)
        {
            ModelState.Remove("Sale");
            if (ModelState.IsValid)
            {
                product.Type_ID = 2;
                product.Sale = 1;
                var user = Session["user"] as user;
                product.User_ID = user.ID;
                product.Post_Time = DateTime.Now;
                db.products.Add(product);
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
