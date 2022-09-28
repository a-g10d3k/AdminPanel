using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AdminPanel.Data;
using Microsoft.AspNetCore.Identity;
using AdminPanel.Models;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AdminController(ILogger<AccountController> logger, AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult UserManagement()
        {
            return View();
        }

        [HttpGet]
        [Route("{controller}/getusers")]
        public async Task<IActionResult> GetUsers()
        {
            if (_context?.Users == null) return StatusCode(503);
            var users = await _context.Users.Select(u => new
            {
                Id = u.Id,
                Name = u.UserName,
                Email = u.Email,
                LastLogin = u.LastLogin.ToString("dd/MM/yyyy HH:mm:ss"),
                Created = u.Created.ToString("dd/MM/yyyy HH:mm:ss"),
                Blocked = u.Blocked
            }).ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("{controller}/deleteusers")]
        public async Task<IActionResult> DeleteUsers([FromBody] List<string> ids)
        {
            if (_context?.Users == null) return StatusCode(503);
            foreach (var id in ids)
                await DeleteUserById(id);
            return Ok(ids);
        }

        [HttpPost]
        [Route("{controller}/setblockstatus")]
        public async Task<IActionResult> BlockUsers([FromBody] List<string> ids, [FromQuery] bool status)
        {
            if (_context?.Users == null) return StatusCode(503);
            var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
            foreach (var user in users)
            {
                user.Blocked = status;
                if(status == true) await _userManager.UpdateSecurityStampAsync(user);
                _logger.LogInformation($"User {user.UserName} was {(status ? "blocked" : "unblocked")}.");
            }
            await _context.SaveChangesAsync();
            return Ok(ids);
        }

        private async Task DeleteUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return;
            await _userManager.DeleteAsync(user);
        }
    }
}
