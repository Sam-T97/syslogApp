using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace syslogSite.Areas.Identity.Pages.Account.Manage
{
    public class ChangeUserAccessModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        public IEnumerable<IdentityRole> Roles { get; set; }
        public ChangeUserAccessModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
        }
        public void OnGet()
        {
            Roles = _roleManager.Roles.Where(r => r.Name != "Has2FA")
                .ToList();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "User Role")]
            public string Role { get; set; }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var roleMembership = await _userManager.GetRolesAsync(user);
                try
                {
                    roleMembership.Remove("Has2FA");
                }
                catch
                {
                    //User does not have 2FA membership yet so ignore and continue.
                }
                foreach (var role in roleMembership)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                await _userManager.AddToRoleAsync(user, Input.Role);
                var userId = await _userManager.GetUserIdAsync(user);
                _logger.LogInformation("User with ID '{UserId}' had access changed to {Role}", userId, Input.Role);
                return Redirect("~/Index");
            }
            return Page();
        }
    }
}
