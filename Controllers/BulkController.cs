using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CutUsage.Models;

namespace CutUsage.Controllers
{
    public class BulkController : Controller
    {
        private readonly LayRepository _layRepo;
        private readonly MarkerRepository _markerRepo;

        public BulkController(
            LayRepository layRepo,
            MarkerRepository markerRepo)
        {
            _layRepo = layRepo;
            _markerRepo = markerRepo;
        }

        // GET: /Bulk/SelectLay
        [HttpGet]
        public async Task<IActionResult> SelectLay()
        {
            // 1) load only LayMaster records where LayType==2 ("bulk")
            var allLays = await _layRepo.GetAllLaysAsync();
            var bulkLays = allLays
              .Where(l => l.LayType == 2)
              .ToList();

            // 2) load marker names so we can show "LayID – MarkerName"
            var markers = await _markerRepo.GetAllAsync();

            // 3) group by LayID, then project
            var options = bulkLays
                .GroupBy(l => l.LayID)
                .Select(g =>
                {
                    var l = g.First();
                    var name = markers
                        .FirstOrDefault(m => m.MarkerId == l.MarkerId)
                        ?.MarkerName
                        ?? "<unknown>";
                    return new
                    {
                        LayID = l.LayID,
                        Display = $"{l.LayID} – {name}"
                    };
                })
                .ToList();

            ViewBag.LayList = new SelectList(options, "LayID", "Display");
            return View();
        }


        // POST: /Bulk/SelectLay
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SelectLay(int layId)
        {
            if (layId <= 0)
            {
                ModelState.AddModelError("layId", "Please select a lay.");
                return View();
            }
            // Redirect to the RollDetails action on LayController
            return RedirectToAction("RollDetails", "Lay", new { id = layId });
        }

    }
}
