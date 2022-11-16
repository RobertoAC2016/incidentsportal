using incidents.Models;
using incidents.Services;
using Microsoft.AspNetCore.Mvc;

namespace incidents.Controllers
{
    public class DataController : Controller
    {
        private DB db;
        public DataController(IConfiguration conf)
        {
            db = new DB(conf);
        }
        //[PermisosRol("admin,supervisor,empleado")]
        public ActionResult<List<incident>> Index()
        {
            List<incident> tts = db.get_incidents();
            return View(tts);
        }
        //[PermisosRol("admin")]
        public ActionResult<List<registro>> Users(String search = null)
        {
            List<registro> users = new List<registro>();
            if (!string.IsNullOrEmpty(search))
            {
                users = db.get_filter_users(search);
            }
            else
            {
                users = db.get_users();
            }
            return View(users);
        }
        public ActionResult<registro> Details(String id)
        {
            List<registro> users = db.get_users(id);
            if (users.Count > 0)
                return View(users[0]);
            else
                return RedirectToAction("Index", "Error");
        }
        public ActionResult<registro> Delete(registro reg)
        {
            bool status = db.delete_user(reg.idatencion.ToString());
            if (status)
                return RedirectToAction("Users");
            else
                return RedirectToAction("Index", "Error");
        }
        public ActionResult<registro> Edit(String id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                List<registro> users = db.get_users(id);
                if (TempData["msg"] != null)
                {
                    ViewBag.Message = TempData["msg"];
                    TempData["msg"] = null;
                }
                if (users.Count > 0)
                    return View(users[0]);
                else
                    return RedirectToAction("Index", "Error");
            }
            else
                return RedirectToAction("Index", "Error");
        }
        public IActionResult UpdateUserData(registro reg)
        {
            TempData["msg"] = null;
            var status = db.Update_Data_User(reg);
            var msg = "";
            switch (status)
            {
                case "error": msg = "Can't update user data"; break;
                case "notexist": msg = "User not exists"; break;
                case "updateerror": msg = "Update fails"; break;
                default: msg = ""; break;
            }
            if (string.IsNullOrEmpty(msg))
                return RedirectToAction("Users", "Data");
            else
            {
                TempData["msg"] = msg;
                return RedirectToAction($"Edit", "Data", reg.idatencion);
            }
        }
    }
}
