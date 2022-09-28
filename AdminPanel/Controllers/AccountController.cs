using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Identity;

namespace AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return LocalRedirect("/");
            return View();
        }

        [HttpPost]
        [Route("{controller}/Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (!ModelState.IsValid) return View("Login", model);
            var user = await _userManager.FindByNameAsync(model.Username);
            if(user != null && user.Blocked)
            {
                ModelState.AddModelError(string.Empty, "You've been blocked from accessing your account.");
                return View("Login", model);
            }
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, true);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {User.Identity!.Name} logged in.");
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();
                }
                return LocalRedirect("/");
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning($"User {User.Identity!.Name} locked out.");
                ModelState.AddModelError(String.Empty, "You've been locked out of your account for failing too many login attempts. Please try again later.");
                return View("Login", model);
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Login", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated) return LocalRedirect("/");
            return View();
        }


        [HttpPost]
        [Route("{controller}/Register")]
        public async Task<IActionResult> RegisterPost(RegisterModel model)
        {
            if (!ModelState.IsValid) return View("Register", model);
            var user = new ApplicationUser 
            { 
                UserName = model.Username,
                Email = model.Email,
                LastLogin = DateTime.Now,
                Created = DateTime.Now
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation($"User {user.UserName} was created.");
                return LocalRedirect("/");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Register", model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if(!User.Identity!.IsAuthenticated) return LocalRedirect("/");
            await _signInManager.SignOutAsync();
            _logger.LogInformation($"User {User.Identity!.Name} logged out.");
            return LocalRedirect("/");
        }
    }
}
