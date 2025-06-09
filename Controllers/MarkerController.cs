using Microsoft.AspNetCore.Mvc;
using CutUsage.Models;
using System.Threading.Tasks;

namespace CutUsage.Controllers
{
    public class MarkerController : Controller
    {
        private readonly MarkerRepository _repo;

        public MarkerController(IConfiguration cfg)
        {
            _repo = new MarkerRepository(cfg);
        }

        // GET: /Marker
        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        // GET: /Marker/Create
        public IActionResult Create()
        {
            return View(new Marker
            {
                MarkerId = "-1" // Default to -1 for create
            });
        }

        // POST: /Marker/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Marker m)
        {
            if (!ModelState.IsValid)
                return View(m);

            await _repo.CreateAsync(m); // returns and sets MarkerId internally
            return RedirectToAction(nameof(Index));
        }

        // GET: /Marker/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null)
                return NotFound();

            return View("Create", m); // Reuse Create.cshtml
        }

        // POST: /Marker/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marker m)
        {
            if (!ModelState.IsValid)
                return View("Create", m);

            await _repo.UpdateAsync(m);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Marker/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null)
                return NotFound();

            return View(m);
        }

        // POST: /Marker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
