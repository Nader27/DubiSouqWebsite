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
    public class ShopController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Shop()
        {
            return View();
        }

        //GET: /Shop/Index/id?(search="5",page="0",sort="5",amount="5-6")
        public ActionResult Index(int? id, string search = "", int page = 0, string sort = "abc", string amount = "")
        {
            List<product> products = new List<product>();
            if (id == null)
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 1).ToList();
            else
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 1).Where(p => p.Category_ID == id).ToList();
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

        //AJAX: /Shop/AddToCart/5
        public PartialViewResult AddToCart(int id)
        {
            int quantity = 1;
            if (Request["quantity"] != null)
                quantity = int.Parse(Request["quantity"]);
            ShoppingCart.AddToCart(id, quantity);
            return PartialView("_CartMenu");
        }

        //Note: Same as AddToCart but only works in Product_Details Page
        //AJAX: /Shop/Product_Details/5
        [HttpPost]
        public PartialViewResult Product_Details(int id)
        {
            int quantity = 1;
            if (Request["quantity"] != null)
                quantity = int.Parse(Request["quantity"]);
            ShoppingCart.AddToCart(id, quantity);
            return PartialView("_CartMenu");
        }

        //AJAX: /Shop/RemoveOneFromCart/5
        public PartialViewResult RemoveOneFromCart(int id)
        {
            ShoppingCart.RemoveFromCart(id,1);
            return PartialView("_CartMenu");
        }

        //AJAX: /Shop/RemoveFromCart/5
        public ActionResult RemoveFromCart(int id)
        {
            user user = Session["user"] as user;
                int quantity = db.cart_item.SingleOrDefault(c => c.user_ID == user.ID && c.Product_ID == id).Quantity;
            ShoppingCart.RemoveFromCart(id, quantity);
            var results = new ShoppingCartViewModel
            {
                CartTotal = ShoppingCart.GetTotal(),
                CartCount = ShoppingCart.GetCount(),
                Id = id
            };
            return Json(results);
        }

        //GET: /Shop/Checkout
        public ActionResult Checkout()
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            Entities db = new Entities();
            ViewBag.Payment_Method = new SelectList(db.payment_method, "ID", "Method");
            return View();
        }

        //GET: /Shop/Cart
        public ActionResult Cart()
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            ViewBag.Cart = ShoppingCart.GetCartItems();
            ViewBag.Total = ShoppingCart.GetTotal();
            return View();
        }

        //GET: /Shop/Wishlist
        public ActionResult Wishlist()
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            ViewBag.Wishlist = ShoppingCart.Getwishlist();
            return View();
        }

        //AJAX: /Shop/Addtowish/5
        [HttpPost]
        public ActionResult Addtowish(int id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            product addedproduct = db.products.Find(id);
            ShoppingCart.AddToWish(id);
            user USER = Session["user"] as user;
            List<whish_list> wl = db.whish_list.Where(W => W.User_ID == USER.ID).ToList();
            var results = new ShoppingCartViewModel
            {
                wishcount = wl.Count,
            };
            return Json(results);
        }

        //AJAX: /Shop/RemoveFromwish/5
        [HttpPost]
        public ActionResult RemoveFromwish(int id)
        {
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            Entities db = new Entities();
            ShoppingCart.RemoveFromWishlist(id);
            user USER = Session["user"] as user;
            List<whish_list> wl = db.whish_list.Where(W => W.User_ID == USER.ID).ToList();
            var results = new ShoppingCartViewModel
            {
                wishcount = wl.Count,
                Id = id
            };
            return Json(results);
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

        //POST: /Shop/Review/5
        [HttpPost]
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
            return RedirectToAction("Product_Details", new { id=id });
        }
    }
}