using DubiSouqWebsite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;

namespace DubiSouqWebsite.Controllers
{
    public class HomeController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Home()
        {
            List<product> products = db.products.Where(p => p.Type_ID == 1).ToList();
            ViewBag.saleproduct = products.OrderByDescending(p => p.Sale).Take(10).ToList();
            ViewBag.newproduct = products.OrderByDescending(p => p.Post_Time).Take(10).ToList();
            ViewBag.cheapproduct = products.OrderBy(p => p.Price).Take(10).ToList();
            List <KeyValuePair< product, int>> Dict = new List<KeyValuePair<product, int>>();
            foreach (product product in products)
            {
                int count = 0;
                int rate = 0;
                List<review> reviews = db.reviews.Where(r => r.Product_ID == product.ID).ToList();
                foreach (review review in reviews)
                {
                    rate += review.Rate;
                    count++;
                }
                int result = 0;
                if (count > 0)
                    result = rate / count;
                Dict.Add( new KeyValuePair<product, int>(product, result));
            }
            Dict = Dict.OrderBy(p => p.Value).Take(10).ToList();
            List<product> _product = new List<product>();
            foreach(KeyValuePair<product, int> item in Dict)
            {
                _product.Add(item.Key);
            }
            ViewBag.ratedproduct = _product;
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Feedback()
        {
            return View();
        }

        public ActionResult Redirect()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Offers()
        {
            return View();
        }

        public ActionResult LoginForm()
        {
            return View();
        }

        public ActionResult RegisterForm()
        {
            return View();
        }


    }
}