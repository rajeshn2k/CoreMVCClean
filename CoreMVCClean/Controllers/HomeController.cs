using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Core.Library.Clean.AdditionalService;

namespace Core.MVC.Clean.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            var exceptionFeature =
                HttpContext.Features.Get<IExceptionHandlerFeature>();

            var exception = exceptionFeature?.Error;

            var path = exceptionFeature?.Path
                       ?? HttpContext.Request.Path;

            var correlationId = HttpContext.GetCorrelationId();

            var model = new ErrorViewModel
            {
                RequestId = correlationId,
                Path = path,
                Exception = exception
            };

            return View(model);
        }
    }
}
