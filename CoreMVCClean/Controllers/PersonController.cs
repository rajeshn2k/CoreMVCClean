using Core.Library.Clean.AdditionalService;
using Microsoft.AspNetCore.Mvc;
using Core.MVC.Clean;

namespace Core.MVC.Clean.Controllers
{
    public class PersonController : Controller
    {
        private readonly PersonDirector apiClient;

        public PersonController(PersonDirector apiClient)
        {
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                IEnumerable<PersonDTO> result = null;

                if (!string.IsNullOrWhiteSpace(search))
                {
                   search = $"?search={Uri.EscapeDataString(search)}";

                   result = await apiClient.SearchEntitiesAsync(search, default).ConfigureAwait(true);
                   return View(result);
                }

                result = await apiClient.GetEntitiesAsync(default).ConfigureAwait(true);
                return View(result);
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

        public async Task<ActionResult> Edit(string personId)
        {
            try
            {
                PersonDTO person = await apiClient.GetEntityByIdAsync(personId, default).ConfigureAwait(true);
                return View(person);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

        [HttpPost]
        public async Task<ActionResult> Edit(string id, [FromForm] PersonDTO person)
        {
            try
            {
                await apiClient.UpdateEntityByIdAsync(id, person, default).ConfigureAwait(true);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string personId)
        {
            try
            {
                await apiClient.DeleteEntityByIdAsync(personId, default).ConfigureAwait(true);
                return RedirectToAction("Index");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

        // GET: Person/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm] PersonCreateDTO person)
        {
            if (!ModelState.IsValid)
            {
                return View(person);
            }

            try
            {
                await apiClient.CreateEntityAsync(person, default).ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
    }
}