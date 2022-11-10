using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace incidents.Controllers
{
    public class ErrorController : Controller
    {
        // GET: ErrorController
        public ActionResult Index()
        {
            return View();
        }
    }
}
