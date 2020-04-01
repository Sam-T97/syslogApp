using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class UpdateHBIntervalModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        public string message;

        public UpdateHBIntervalModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AppVars AppVars { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {

            AppVars = await _context.appvars.FirstOrDefaultAsync(m => m.ID == 1);

            if (AppVars == null)
            {
                //Something went really wrong 
                return NotFound();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            int valueTest;
            try
            {
                valueTest = int.Parse(AppVars.Value);
            }
            catch
            {
                message = "You must enter a valid whole number for the interval";
                return Page();
            }

            if (valueTest <= 0)
            {
                message = "You must enter a number above 0 for the interval";
                return Page();
            }
            AppVars.VariableName = "HBInterval";
            _context.Attach(AppVars).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppVarsExists(AppVars.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
        private bool AppVarsExists(int id)
        {
            return _context.appvars.Any(e => e.ID == id);
        }
    }
}
