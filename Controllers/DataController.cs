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
        public ActionResult<List<registro>> Users()
        {
            List<registro> users = db.get_users();
            return View(users);
        }
        public ActionResult<registro> Details(registro reg)
        {
            List<registro> users = db.get_users(reg.idatencion.ToString());
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
    }
}
