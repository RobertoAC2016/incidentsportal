using incidents.Models;
using Microsoft.AspNetCore.Mvc;

namespace incidents.Controllers
{
    public class AdminController : Controller
    {
        private DB db;
        public AdminController(IConfiguration conf)
        {
            db = new DB(conf);
        }
        public ActionResult<registro> Register(response_sql msg = null)
        {
            if (msg != null) ViewBag.Message = msg.message;
            var obj = new registro();
            obj.departamentos = db.get_departments();
            obj.roles = db.get_roles();
            return View(obj);
        }
        public ActionResult<response_sql> SaveNewRegister(registro usr)
        {
            if (string.IsNullOrEmpty(usr.empleado))
            {
                return RedirectToAction("Register", "Admin", new response_sql { message = "Some fields are required"});
            }
            else
            {
                var status = db.Save_New_User(usr);
                response_sql resp = new response_sql
                {
                    valor = usr.empleado,
                };
                switch (status)
                {
                    case "error": resp.message = "with error, can't create account"; break;
                    case "privileges": resp.message = "error to add privileges"; break;
                    case "account": resp.message = "error to create account"; break;
                    case "duplicate": resp.message = "error account duplicated"; break;
                    default: resp.message = "account successful"; break;
                }
                if (resp.message.Contains("success"))
                    return RedirectToAction("Success", "Admin", resp);
                else
                    return RedirectToAction("Register", "Admin", resp);
            }
        }
        public ActionResult<response_sql> Success(response_sql obj)
        {
            return View(obj);
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
