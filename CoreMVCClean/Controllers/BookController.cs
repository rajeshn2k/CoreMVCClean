using Core.Library.Clean.AdditionalService;
using Core.Library.Clean.AdditionalService.Resilience;
using Microsoft.AspNetCore.Mvc;
using Core.MVC.Clean;

namespace Core.MVC.Clean.Controllers
{
    public class BookController : Controller
    {
        private readonly BookDirector apiClient;

        public BookController(BookDirector apiClient)
        {
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                IEnumerable<BookDTO> result = null;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"?search={Uri.EscapeDataString(search)}";

                    result = await apiClient.SearchEntitiesAsync(search, default).ConfigureAwait(true);
                    return View(result);
                }

                result = await apiClient.GetEntitiesAsync(default).ConfigureAwait(false);
                return View(result);
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return View("Error", errorViewModel);
            }
        }

        public async Task<ActionResult> Edit(string bookId)
        {
            try
            {
                BookDTO book = await apiClient.GetEntityByIdAsync(bookId, default).ConfigureAwait(false);
                return View(book);
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, [FromForm] BookDTO book)
        {
            try
            {
                await apiClient.UpdateEntityByIdAsync(id, book, default).ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return View(book);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string bookId)
        {
            try
            {
                await apiClient.DeleteEntityByIdAsync(bookId, default).ConfigureAwait(true);
                return RedirectToAction("Index");
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Book/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm] BookCreateDTO book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            try
            {
                await apiClient.CreateEntityAsync(book, default).ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            catch (CircuitBreakerOpenException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = "Book API is currently unavailable due to circuit breaker activation. Please try again later.",
                    ErrorCode = ErrorCodes.EXTERNAL_API_UNAVAILABLE,
                    StatusCode = 503
                };
                return View("CircuitBreakerOpen", errorViewModel);
            }
            catch (ApiException ex)
            {
                var correlationId = HttpContext.GetCorrelationId();
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = correlationId,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode,
                    StatusCode = ex.StatusCode
                };
                return View(book);
            }
        }
    }
}