using Core.Library.Clean.AdditionalService;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<ActionResult> Edit(string bookId)
        {
            BookDTO book = await apiClient.GetEntityByIdAsync(bookId, default).ConfigureAwait(false);
            return View(book);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, [FromForm] BookDTO book)
        {
            await apiClient.UpdateEntityByIdAsync(id, book, default).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string bookId)
        {
            await apiClient.DeleteEntityByIdAsync(bookId, default).ConfigureAwait(true);
            return RedirectToAction("Index");
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

            await apiClient.CreateEntityAsync(book, default).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }
    }
}