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

        //GET: /Shop/Index/id?(search="5",page="0",sort="5",amount="5-6")
        public ActionResult Index(int? id, string search = "", int page = 0 ,string sort = "abc",string amount="")
        {
            List<product> products = new List<product>();
            if (id == null)
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 1).ToList();
            else
                products = db.products.Include(p => p.category).Include(p => p.product_picture).Where(p => p.Type_ID == 1).Where(p=>p.Category_ID == id).ToList();
            if (search != "")
                products = products.Where(p => p.Name.Contains(search) || p.category.Name.Contains(search) || p.Description.Contains(search)).ToList();
            if(amount != "")
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
            if(categoory.category2 != null)
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
            ShoppingCart.AddToCart(id);
            return PartialView();
        }

        //AJAX: /Shop/RemoveFromCart/5
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            Entities db = new Entities();
            ShoppingCart.RemoveFromCart(id);
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
                return RedirectToAction("Login", "User");
            Entities db = new Entities();
            ShoppingCart.RemoveFromWishlist(id);
            user USER = Session["user"] as user;
            List<whish_list> wl = db.whish_list.Where(W=>W.User_ID == USER.ID).ToList();
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
            if (Session["user"] == null)
                return RedirectToAction("Index", "User");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            product product = db.products.Include(p => p.product_picture).Include(p => p.reviews).SingleOrDefault(p => p.ID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    }
}