using Microsoft.AspNetCore.Mvc;

namespace CutUsage.Controllers
{
    public class AccountController : Controller
    {
        private readonly DocketRepository _repository; // You might want to extract user-related methods in a separate UserRepository

        public AccountController(IConfiguration configuration)
        {
            _repository = new DocketRepository(configuration);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Call your stored procedure spValidateUser using similar ADO.NET code.
            // If valid, set authentication cookie/session.
            // For this sample, assume validation is successful.
            if (username == "test" && password == "test") // Replace with actual validation
            {
                // set cookie or session
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }
        }
    }

}
