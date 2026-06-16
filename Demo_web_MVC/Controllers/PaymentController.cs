using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
