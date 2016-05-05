using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace DubiSouqWebsite.Controllers
{
    public class SendMailer2Controller : Controller
    {
        // GET: SendMailer2
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ViewResult Index(DubiSouqWebsite.Models.MailModel _objModelMail)
        {
            if (ModelState.IsValid)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(_objModelMail.To);
                mail.From = new MailAddress(_objModelMail.From);
                mail.Subject = _objModelMail.Subject;
                string Body = _objModelMail.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                ("dubisouq@gmail.com", "DubiSouqAdmin");// Enter seders User name and password
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return View("Index", _objModelMail);
            }
            else
            {
                return View();
            }
        }
    }
}