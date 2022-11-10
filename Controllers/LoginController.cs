using incidents.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, obj.name),
                        new Claim("username", obj.login),
                    };
                    foreach (var role in obj.access)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var claimidentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimidentity));
                    //HttpContext.Session.SetString("username", obj.login);
                    //HttpContext.Session.SetString("token", obj.token);
                    //HttpContext.Session.SetString("fullname", obj.name);
                    return RedirectToAction("Index", "Home");
                }
            }
        }
    }
}
