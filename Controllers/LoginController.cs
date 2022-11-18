using incidents.Models;
using Microsoft.AspNetCore.Mvc;

namespace incidents.Controllers
{
    public class LoginController : Controller
    {
        private security SEC = new security();
        private DB db;
        public LoginController(IConfiguration conf)
        {
            db = new DB(conf);
        }
        public ActionResult<login_request> Index(login_request req = null)
        {
            ViewBag.Message = "";
            if (!req.message.Equals("")) ViewBag.Message = req.message;
            return View(req);
        }
        public async Task<IActionResult> Login(login_request req)
        {
            if (req.username.Equals("") || req.password.Equals(""))
            {
                req.message = "User or password empty";
                return RedirectToAction("Index", "Login", req);
            }
            else
            {
                var obj = db.validate_credentials(req);
                if (!obj.autenticated)
                {
                    req.message = obj.message;
                    return RedirectToAction("Index", "Login", req);
                }
                else
                {
                    HttpContext.Session.SetString("username", obj.login);
                    HttpContext.Session.SetString("fullname", obj.name);
                    HttpContext.Session.SetString("role", obj.role);
                    return RedirectToAction("Index", "Data");
                }
            }
        }
    }
}
