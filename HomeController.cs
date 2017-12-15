using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WWD.Models;
namespace WWD.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        DonationDB ctx = new DonationDB();
        public ActionResult Index()
        {
            var ca = ctx.Kampanyas.Where(x => x.OnaylandiMi == true).ToList();
            return View(ca);
        }
    }
}