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

        public async Task<ActionResult> Edit(string personId)
        {
            PersonDTO person = await apiClient.GetEntityByIdAsync(personId, default).ConfigureAwait(true);
            return View(person);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, [FromForm] PersonDTO person)
        {
            await apiClient.UpdateEntityByIdAsync(id, person, default).ConfigureAwait(true);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string personId)
        {
            await apiClient.DeleteEntityByIdAsync(personId, default).ConfigureAwait(true);
            return RedirectToAction("Index");
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

            await apiClient.CreateEntityAsync(person, default).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }
    }
}