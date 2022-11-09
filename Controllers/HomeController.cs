using incidents.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace incidents.Controllers
{
    public class HomeController : Controller
    {
        public LoginInfo ses;
        private DB db;
        public HomeController(IConfiguration conf)
        {
            db = new DB(conf);
        }
        public ActionResult<LoginInfo> Index()
        {
            if (ses == null)
                return View();
            else
            {
                ses.username = "roberto";
                return View(ses);
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}