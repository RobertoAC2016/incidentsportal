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
    }
}
