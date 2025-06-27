using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CutUsage.Models;

namespace CutUsage.Controllers
{
    public class LayController : Controller
    {
        private readonly LayRepository _layRepo;
        private readonly MarkerRepository _markerRepo;
        private readonly MarkerPlanRepository _markerPlanRepo;

        public LayController(
            LayRepository layRepo,
            MarkerRepository markerRepo,
            MarkerPlanRepository markerPlanRepo)
        {
            _layRepo = layRepo;
            _markerRepo = markerRepo;
            _markerPlanRepo = markerPlanRepo;
        }

        // GET: /Lay/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
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
                ViewData["Markers"] = await _markerRepo.GetAllAsync();
                ViewData["Types"] = await _layRepo.GetLayTypesAsync();
                ViewData["Tables"] = await _layRepo.GetLayTablesAsync();
                return View(model);
            }

            await _layRepo.InsertLayMasterAsync(model);
            return RedirectToAction("Index");
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

            var details = await _layRepo.GetLayDetailsAsync(id);

            if (details.Any(d => !string.IsNullOrEmpty(d.SO)))
            {
                var soList = string.Join(",", details.Select(d => d.SO).Distinct());
                var sizeDetails = await _layRepo.GetLaySODetailsAsync(soList);
                var sizes = sizeDetails.Select(x => x.SOSize).Distinct().ToList();
                var qtyMap = sizeDetails.GroupBy(x => x.SOSize)
                                         .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));
                var totalQty = qtyMap.Values.Sum();

                ViewBag.Sizes = sizes;
                ViewBag.QtyMap = qtyMap;
                ViewBag.TotalQty = totalQty;
            }

            ViewBag.Dockets = await _layRepo.GetDocketsAsync(master.LayType, master.Style, master.LayID);
            ViewBag.Details = details;

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
                .FirstOrDefault(m => m.MarkerId == master.MarkerId)?.MarkerName ?? "(unknown)";

            ViewData["Dockets"] = await _layRepo.GetDocketsAsync(master.LayType, master.Style, master.LayID);
            return View(master);
        }

        // POST: /Lay/Assign
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int layID, string selected)
        {
            var master = await _layRepo.GetLayByIdAsync(layID);
            if (master == null) return NotFound();

            var detail = new LayDetail { LayID = layID };
            var parts = selected.Split('|');
            if (master.LayType == 1)
            {
                detail.DocketNo = parts[0];
                detail.SO = parts[1];
            }
            else
            {
                detail.SO = parts[0];
                detail.DocketNo = string.Empty;
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

        // POST: /Lay/SaveRollDetails
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRollDetails(LayRollDetailsViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("RollDetails", vm);

            try
            {
                var message = await _layRepo.UpsertCutUsageValuesAsync(vm);
                TempData["CutUsageMessage"] = message;
                return RedirectToAction(nameof(RollDetails), new { id = vm.Header.LayID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

        // GET: /Lay/RollDetails/5
        [HttpGet]
        public async Task<IActionResult> RollDetails(int id)
        {
            var vm = await _layRepo.GetLayRollDetailsAsync(id);
            if (vm.Header.LayID == 0) return NotFound();
            return View(vm);
        }

        // GET: /Lay/CreateMarkerPlan
        [HttpGet]
        public async Task<IActionResult> CreateMarkerPlan()
        {
            ViewBag.Styles = await _layRepo.GetAllStyles();
            return View(new MarkerPlanCreateViewModel());
        }

        // Controllers/LayController.cs

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMarkerPlan(MarkerPlanCreateViewModel vm)
        {
            // Step 0: basic header validation
            if (vm.SelectedSO == null || vm.SelectedSO.Length == 0)
                ModelState.AddModelError(nameof(vm.SelectedSO), "Please select at least one SO.");
            if (vm.SelectedDocket == null || vm.SelectedDocket.Length == 0)
                ModelState.AddModelError(nameof(vm.SelectedDocket), "Please select at least one Docket.");

            // Step 1: re-fetch the matrix metadata
            var soList = string.Join(",", vm.SelectedSO);
            var sizeDetails = await _layRepo.GetLaySODetailsAsync(soList);
            var sizes = sizeDetails.Select(x => x.SOSize).Distinct().ToList();
            var qtyMap = sizeDetails
                .GroupBy(x => x.SOSize)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));
            var existingMap = await _layRepo.GetExistingCutBySizeAsync(0);
            var dockets = vm.SelectedDocket.ToList();
            var infos = await _layRepo.GetDocketMaterialInfoAsync(dockets);
            var matMap = infos.ToDictionary(i => i.DocketNo, i => i.MaterialCode);
            var bomMap = infos.ToDictionary(i => i.DocketNo, i => i.BOMUsage);

            // Step 2: build the Details list by pulling each input by name
            var form = Request.Form;
            vm.Details = new List<MarkerPlanDetailViewModel>();
            foreach (var d in dockets)
            {
                // marker‐level inputs
                var markerName = form[$"MarkerName[{d}]"].FirstOrDefault() ?? "";
                var markerLength = decimal.TryParse(form[$"MarkerLength[{d}]"].FirstOrDefault(), out var ml) ? ml : 0m;
                var markerWidth = decimal.TryParse(form[$"MarkerWidth[{d}]"].FirstOrDefault(), out var mw) ? mw : 0m;
                var allowance = decimal.TryParse(form[$"Allowance[{d}]"].FirstOrDefault(), out var al) ? al : 0m;

                // size‐level ratios and totals
                foreach (var s in sizes)
                {
                    // ratio input: how many panels of size s in this marker
                    var ratioKey = $"Ratios[{d}][{s}]";
                    var ratio = int.TryParse(form[ratioKey].FirstOrDefault(), out var r) ? r : 0;

                    // total‐plies input: user can override or just use sum of ratios
                    var totalKey = $"Totals[{d}]";
                    var totalPlies = int.TryParse(form[totalKey].FirstOrDefault(), out var t) ? t : ratio;

                    // server‐side lookups
                    var orderQty = qtyMap.GetValueOrDefault(s, 0m);
                    var cutQty = existingMap.GetValueOrDefault(s, 0m);
                    var matCd = matMap.GetValueOrDefault(d, "");
                    var bomUsage = bomMap.GetValueOrDefault(d);

                    // recalc everything using the user’s totalPlies
                    var fabricReq = totalPlies * markerLength + allowance * totalPlies;
                    var usage = orderQty > 0 ? fabricReq / orderQty : 0m;
                    var saving = (bomUsage - usage) * orderQty;
                    var target = orderQty * bomUsage;

                    vm.Details.Add(new MarkerPlanDetailViewModel
                    {
                        DocketNo = d,
                        Size = s,
                        Qty = ratio,
                        ExistingCutQty = cutQty,
                        MaterialCode = matCd,
                        BOMUsage = bomUsage,
                        NoOfPlies = totalPlies,
                        FabricRequirement = fabricReq,
                        MarkerUsage = usage,
                        MarkerSaving = saving,
                        TargetLength = target,
                        MarkerName = markerName,
                        MarkerLength = markerLength,
                        MarkerWidth = markerWidth,
                        Allowance = allowance
                    });
                }
            }

            // Step 3: ensure we actually got some rows
            if (!vm.Details.Any())
                ModelState.AddModelError(nameof(vm.Details), "Please enter at least one size/quantity row.");

            if (!ModelState.IsValid)
            {
                ViewBag.Styles = await _layRepo.GetAllStyles();
                return View(vm);
            }

            // Step 4: persist
            var newPlanId = await _markerPlanRepo.CreateAsync(vm);
            TempData["Success"] = $"Marker plan #{newPlanId} saved.";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Lay/GetSOsByStyle?style=ST1&style=ST2
        [HttpGet]
        public async Task<JsonResult> GetSOsByStyle([FromQuery] string[] style)
        {
            // if your repo only takes one style at a time, just loop:
            var all = new List<string>();
            foreach (var st in style)
                all.AddRange(await _layRepo.GetSOsByStyleAsync(st));

            var distinct = all.Distinct();

            return Json(distinct.Select(so => new { value = so, text = so }));
        }

        // AJAX: Get Dockets by SO
        [HttpGet]
        public async Task<JsonResult> GetDocketsBySO(string so)
        {
            var dockets = await _layRepo.GetDocketsBySOAsync(so);
            return Json(dockets.Select(d => new { value = d.DocketNo, text = d.DocketNo }));
        }

        // AJAX: Build marker-plan matrix (GET)
        [HttpGet]
        public async Task<PartialViewResult> BuildPlanMatrix([FromQuery] string[] so, [FromQuery] string[] docket)
        {
            // size breakdown
            var sizeDetails = await _layRepo.GetLaySODetailsAsync(string.Join(",", so));
            var sizes = sizeDetails.Select(x => x.SOSize).Distinct().ToList();
            var qtyMap = sizeDetails.GroupBy(x => x.SOSize).ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));
            var totalQty = qtyMap.Values.Sum();

            // existing cuts
            var existingCutMap = await _markerPlanRepo.GetExistingCutBySOAsync(so);
            var existingCutTotal = existingCutMap.Values.Sum();

            // 1) fetch material & bom as before
            var infos = await _layRepo.GetDocketMaterialInfoAsync(docket);
            var matMap = infos.ToDictionary(i => i.DocketNo, i => i.MaterialCode);
            var bomMap = infos.ToDictionary(i => i.DocketNo, i => i.BOMUsage);

            // 2) **NEW**: fetch your ratios
            var ratioList = await _layRepo.GetDocketSizeRatiosAsync(docket);
            var ratioMap = ratioList
                .GroupBy(r => r.DocketNo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                   g => g.Key,
                   g => g.ToDictionary(x => x.Size, x => x.Ratio, StringComparer.OrdinalIgnoreCase),
                   StringComparer.OrdinalIgnoreCase
                );

            var vm = new MarkerPlanMatrixViewModel
            {
                Sizes = sizes,
                QtyMap = qtyMap,
                TotalQty = totalQty,
                ExistingCutMap = existingCutMap,
                Dockets = docket.ToList(),
                MaterialCodeMap = matMap,
                BOMUsageMap = bomMap,
                RatioMap = ratioMap      // ← set it here
            };

            return PartialView("_BuildPlanMatrix", vm);
        }
    }
}
