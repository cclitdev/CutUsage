using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CutUsage.Models;

namespace CutUsage.Controllers
{
    public class MarkerController : Controller
    {
        private readonly MarkerRepository _repo;
        public MarkerController(IConfiguration cfg)
            => _repo = new MarkerRepository(cfg);

        // GET: /Marker
        public async Task<IActionResult> Index()
            => View(await _repo.GetAllAsync());

        // GET: /Marker/Create
        public IActionResult Create()
            => View(new Marker());

        // POST: /Marker/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Marker m)
        {
            if (!ModelState.IsValid) return View(m);
            await _repo.CreateAsync(m);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Marker/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return NotFound();
            // explicitly render the Create.cshtml view
            return View("Create", m);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marker m)
        {
            if (!ModelState.IsValid)
                return View("Create", m);

            await _repo.UpdateAsync(m);
            return RedirectToAction(nameof(Index));
        }


        // GET: /Marker/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return NotFound();
            return View(m);
        }

        // POST: /Marker/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }



    }
}
