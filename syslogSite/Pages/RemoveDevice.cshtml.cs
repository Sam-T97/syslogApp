using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class RemoveDeviceModel : PageModel
    {
        public static string feedback;
        private readonly SyslogShared.ApplicationDbContext _context;

        public RemoveDeviceModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string host)
        {
            if (id == null && host == null)
            {
                return Page();
            }

            Device = await _context.Devices.FirstOrDefaultAsync(m => m.ID == id || m.HostName == host);

            if (Device == null)
            {
                feedback = "No device found with these details. Check the details are correct and try again";
                return Page();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices.FindAsync(id);

            if (Device != null)
            {
                _context.Devices.Remove(Device);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetLocateAsync(int? id, string? host)
        {
            Device = _context.Devices.FirstOrDefault(m => m.ID == id || m.HostName == host);
            if (Device == null)
            {
                feedback = "No device found with these details. Check the details are correct and try again";
                return Page();
            }

            return Page();
        }
    }
}
