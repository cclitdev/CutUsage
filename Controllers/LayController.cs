﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CutUsage.Models;    // LayMaster, LayDetail, LayCreateViewModel, etc.

namespace CutUsage.Controllers
{
    public class LayController : Controller
    {
        private readonly LayRepository _layRepo;
        private readonly MarkerRepository _markerRepo;

        public LayController(LayRepository layRepo, MarkerRepository markerRepo)
        {
            _layRepo = layRepo;
            _markerRepo = markerRepo;
        }

        // GET: /Lay/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Load raw lists into ViewData
            ViewData["Styles"] = await _layRepo.GetAllStyles();
            ViewData["Markers"] = await _markerRepo.GetAllAsync();
            ViewData["Types"] = await _layRepo.GetLayTypesAsync();
            ViewData["Tables"] = await _layRepo.GetLayTablesAsync();

            return View(new LayMaster());
        }

        // POST: /Lay/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LayMaster model)
        {
            if (!ModelState.IsValid)
            {
                // Re‐load lists on error
                ViewData["Markers"] = await _markerRepo.GetAllAsync();
                ViewData["Types"] = await _layRepo.GetLayTypesAsync();
                ViewData["Tables"] = await _layRepo.GetLayTablesAsync();
                return View(model);
            }

            // Save and redirect
            await _layRepo.InsertLayMasterAsync(model);
            return RedirectToAction("Index");  // or wherever your list lives
        }

        // GET: /Lay
        public async Task<IActionResult> Index()
        {
            var list = await _layRepo.GetAllLaysAsync();
            return View(list);
        }

        // GET: /Lay/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var master = await _layRepo.GetLayByIdAsync(id);
            if (master == null) return NotFound();

            ViewBag.Markers = await _markerRepo.GetAllAsync();
            ViewBag.Types = await _layRepo.GetLayTypesAsync();
            ViewBag.Tables = await _layRepo.GetLayTablesAsync();

            // load assigned details, now including MaterialCode
            var details = await _layRepo.GetLayDetailsAsync(id);

            // if any SOs assigned, fetch size breakdown
            if (details.Any(d => !string.IsNullOrEmpty(d.SO)))
            {
                var soList = string.Join(",", details.Select(d => d.SO).Distinct());
                var sizeDetails = await _layRepo.GetLaySODetailsAsync(soList);

                var sizes = sizeDetails
                                .Select(x => x.SOSize)
                                .Distinct()
                                .ToList();

                var qtyMap = sizeDetails
                                .GroupBy(x => x.SOSize)
                                .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));

                var totalQty = qtyMap.Values.Sum();

                ViewBag.Sizes = sizes;
                ViewBag.QtyMap = qtyMap;
                ViewBag.TotalQty = totalQty;
            }

            ViewBag.Dockets = await _layRepo.GetDocketsAsync(master.LayType, master.Style,master.LayID);
            ViewBag.Details = details;  // each d now has SO, DocketNo and MaterialCode

            return View(master);
        }



        // GET: /Lay/Assign/11
        [HttpGet]
        public async Task<IActionResult> Assign(int id)
        {
            var master = await _layRepo.GetLayByIdAsync(id);
            if (master == null) return NotFound();

            var allMarkers = await _markerRepo.GetAllAsync();
            ViewBag.MarkerName = allMarkers
                .FirstOrDefault(m => m.MarkerId == master.MarkerId)?
                .MarkerName
                ?? "(unknown)";

            // Only need Dockets here
            ViewData["Dockets"] = await _layRepo.GetDocketsAsync(master.LayType, master.Style,master.LayID);

            return View(master);
        }

        // GET: /Lay/AssignPartial?id=123
        [HttpGet]
        public async Task<IActionResult> AssignPartial(int id)
        {
            // 1) ensure the lay exists
            var master = await _layRepo.GetLayByIdAsync(id);
            if (master == null) return NotFound();

            // 2) load the existing SO/Docket details
            var details = await _layRepo.GetLayDetailsAsync(id);

            // 3) figure out which sizes you need columns for
            var sizes = new List<string>();
            if (details.Any(d => !string.IsNullOrEmpty(d.SO)))
            {
                var soList = string.Join(",", details.Select(d => d.SO).Distinct());
                var sizeDetails = await _layRepo.GetLaySODetailsAsync(soList);
                sizes = sizeDetails.Select(x => x.SOSize).Distinct().ToList();
            }

            ViewBag.Sizes = sizes;
            // 4) render the partial with just your “Assigned Dockets” table
            return PartialView("_AssignPartial", details);
        }


        // POST: /Lay/Assign
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int layID, string selected)
        {
            var master = await _layRepo.GetLayByIdAsync(layID);
            if (master == null) return NotFound();

            var detail = new LayDetail { LayID = layID };

            // if it’s a Docket‐type, selected is a DocketNo —
            // so set SO to empty string (never null)
            if (master.LayType == 1)
            {
                var parts = selected.Split('|');
                detail.DocketNo = parts[0];
                detail.SO = parts[1];
            }
            else
            {
                var parts = selected.Split('|');
                detail.SO = parts[0];
                detail.DocketNo = "";      // ← you can also pass empty if you prefer
            }

            await _layRepo.InsertLayDetailAsync(detail);
            return RedirectToAction(nameof(Details), new { id = layID });
        }

        // POST: /Lay/UpdateMaster
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMaster(LayMaster model)
        {
            if (!ModelState.IsValid)
                return await Details(model.LayID);

            await _layRepo.UpdateLayMasterAsync(model);
            return RedirectToAction(nameof(Details), new { id = model.LayID });
        }

        // POST: /Lay/AddDetail
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDetail(int layID, string selected)
        {
            var master = await _layRepo.GetLayByIdAsync(layID);
            if (master == null) return NotFound();

            var detail = new LayDetail { LayID = layID };
            if (master.LayType == 1)
                detail.DocketNo = selected;
            else
                detail.SO = selected;

            await _layRepo.InsertLayDetailAsync(detail);
            return RedirectToAction(nameof(Details), new { id = layID });
        }

        // LayController.cs
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRollDetails(LayRollDetailsViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("RollDetails", vm);

            try
            {
                // this will INSERT or UPDATE each row and return the last proc message
                var message = await _layRepo.UpsertCutUsageValuesAsync(vm);

                // store in TempData so it survives the redirect
                TempData["CutUsageMessage"] = message;

                // redirect back to the GET so the user sees the updated grid
                return RedirectToAction(nameof(RollDetails), new { id = vm.Header.LayID });
            }
            catch (Exception ex)
            {
                // show the error above the form
                ModelState.AddModelError("", ex.Message);
                return View("RollDetails", vm);
            }
        }



        // POST: /Lay/DeleteDetail
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDetail(int layID, string so, string docketNo)
        {
            await _layRepo.DeleteLayDetailAsync(new LayDetail
            {
                LayID = layID,
                SO = so,
                DocketNo = docketNo
            });
            return RedirectToAction(nameof(Details), new { id = layID });
        }

        // Controllers/LayController.cs
        [HttpGet]
        public async Task<IActionResult> RollDetails(int id)
        {
            var vm = await _layRepo.GetLayRollDetailsAsync(id);
            if (vm.Header.LayID == 0)
                return NotFound();

            return View(vm);
        }

    }
}
