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
        //Get the user manager, logger and role manager 
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        //Create this enumerable for the select box in the HTML
        public IEnumerable<IdentityRole> Roles { get; set; }
        //Assign values to everything above on page load
        public ChangeUserAccessModel(
            UserManager<IdentityUser> userManager,
            ILogger<DeletePersonalDataModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }
        //Grab the list of roles in the system and send them to the enumerable
        public void OnGet()
        {
            Roles = _roleManager.Roles.Where(r => r.Name != "Has2FA")
                .ToList();
        }
        //This is the model the user will need to input data for 
        [BindProperty]
        public InputModel Input { get; set; }
        //Set the values needed for this model
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
        //When the user clicks go fire this
        public async Task<IActionResult> OnPostAsync()
        {
            //Do we have all the data we need?
            if (ModelState.IsValid)
            {
                //Get the target user by email address
                var user = await _userManager.FindByEmailAsync(Input.Email);
                //Does the user actually exist?
                if (user == null)
                {
                    //Tell the user off 
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }
                //Grab the users roles
                var roleMembership = await _userManager.GetRolesAsync(user);
                try
                {
                    //Remove the 2FA role from the list 
                    roleMembership.Remove("Has2FA");
                }
                catch
                {
                    //User does not have 2FA membership yet so ignore and continue.
                }
                //Remove the user for their current role
                foreach (var role in roleMembership)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
                //Add the user to the new role we want
                await _userManager.AddToRoleAsync(user, Input.Role);
                //Grab some details for the log and add a log message
                var userId = await _userManager.GetUserIdAsync(user);
                _logger.LogInformation("User with ID '{UserId}' had access changed to {Role}", userId, Input.Role);
                //Kick the user back to the index page
                return Redirect("~/Index");
            }
            //The user missed entering something show the form again 
            return Page();
        }
    }
}
