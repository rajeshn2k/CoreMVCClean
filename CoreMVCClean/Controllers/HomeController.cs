using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Core.MVC.Clean;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string requestId = null)
        {
            var correlationId = requestId ?? HttpContext.GetCorrelationId() ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionFeature?.Error;

            var model = new ErrorViewModel
            {
                RequestId = correlationId,
                ExceptionMessage = exception?.Message,
                ExceptionStackTrace = exception?.StackTrace
            };

            if (exception is ApiException apiException)
            {
                model.Message = apiException.Message;
                model.ErrorCode = apiException.ErrorCode;
                model.StatusCode = apiException.StatusCode;
            }

            return View(model);
        }
    }
}
