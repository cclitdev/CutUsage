using CutUsage.Models;
using CutUsage.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CutUsage.Controllers
{
    public class DocketController : Controller
    {
        private readonly DocketRepository _repository;

        public DocketController(IConfiguration configuration)
        {
            _repository = new DocketRepository(configuration);
        }

        // GET: /Docket/SelectDocket
        [HttpGet]
        public IActionResult SelectDocket()
        {
            // Display a view with a form to enter a docket number.
            return View();
        }

        // POST: /Docket/SelectDocket
        [HttpPost]
        public IActionResult SelectDocket(string docketNo)
        {
            if (string.IsNullOrEmpty(docketNo))
            {
                ModelState.AddModelError("", "Please enter a docket number.");
                return View();
            }
            // Redirect to the Details action with the provided docket number.
            return RedirectToAction("Details", new { docketNo = docketNo });
        }

        // GET: /Docket/Details?docketNo=...
        [HttpGet]
        public async Task<IActionResult> Details(string docketNo)
        {
            if (string.IsNullOrEmpty(docketNo))
            {
                ModelState.AddModelError("", "Please enter a docket number.");
                return RedirectToAction("SelectDocket");
            }

            // Get header details (DocketDetail)
            var header = await _repository.GetDocketDetailsAsync(docketNo);
            if (header == null)
            {
                ModelState.AddModelError("", "No docket details found for the provided number.");
                return RedirectToAction("SelectDocket");
            }

            // Get the usage roles.
            var usageRoles = await _repository.GetUsageRoleDetailsAsync(docketNo);

            var viewModel = new DocketDetailsViewModel
            {
                Header = header,
                UsageRoles = usageRoles
            };

            // Read any success/error message from TempData
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(viewModel);
        }


        // POST: /Docket/SaveCatValues
        [HttpPost]
        public async Task<IActionResult> SaveCatValues(DocketDetailsViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Re-fetch usage roles if model state is invalid.
            //    model.UsageRoles = await _repository.GetUsageRoleDetailsAsync(model.Header.DocketNo);
            //    return View("Details", model);
            //}

            try
            {
                // Loop through each usage role, map to CatValueModel, and save via the stored procedure.
                foreach (var usageRole in model.UsageRoles)
                {
                    var catValueModel = new CatValueModel
                    {
                        DocketNo = usageRole.DocketNo,
                        SystemBatch = usageRole.SystemBatch,
                        Cat1Value = usageRole.Cat1Value,
                        Cat2Value = usageRole.Cat2Value,
                        Cat3Value = usageRole.Cat3Value,
                        Cat4Value = usageRole.Cat4Value
                    };

                    await _repository.InsertCatValuesAsync(catValueModel);
                }

                TempData["SuccessMessage"] = "Category values saved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error saving category values: " + ex.Message;
            }

            // IMPORTANT: Redirect to the GET action, which will fetch fresh data from the DB
            return RedirectToAction("Details", new { docketNo = model.Header.DocketNo });
        }


    }
}
