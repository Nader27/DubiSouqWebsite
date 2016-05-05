using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DubiSouqWebsite.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(String error)
        {
            ViewBag.Error = error;
            return View();
        }
    }
}