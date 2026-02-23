using Microsoft.AspNetCore.Mvc;

namespace testforproject.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
