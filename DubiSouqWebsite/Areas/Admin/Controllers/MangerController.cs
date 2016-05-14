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
    public class MangerController : Controller
    {
        // GET: Admin/Manger
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
        public ActionResult Dashboard()
        {
            if (Session["admin"] == null || (Session["admin"] as user).Type_id != 2)
                return RedirectToAction("index", "Admin");
            return View();
        }
    }
}