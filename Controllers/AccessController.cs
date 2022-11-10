using Microsoft.AspNetCore.Mvc;

namespace incidents.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
