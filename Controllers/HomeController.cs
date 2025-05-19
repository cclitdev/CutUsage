using CutUsage.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CutUsage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DocketRepository _repository;

        // Constructor takes both ILogger and IConfiguration.
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _repository = new DocketRepository(configuration);
        }

        // Optionally, if you want the Login page to be the default landing page,
        // update your default route in Program.cs to: "{controller=Home}/{action=Login}/{id?}"

        // GET: /Home/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            bool isValidUser = await _repository.ValidateUserAsync(username, password);
            if (isValidUser)
            {
                // Optionally set authentication cookies/session here.
                // For demonstration, using a default docket number.
                string defaultDocketNo = "DCKT001"; // Replace with a valid docket number from your database.
                return RedirectToAction("Details", "Docket", new { docketNo = defaultDocketNo });
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }
        }


        // Other actions (Index, Privacy, Error) can be here as needed.
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
