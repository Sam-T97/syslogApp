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
    public class RemoveRemoteDeviceModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public RemoveRemoteDeviceModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int RemoteDeviceID { get; set; }

        public IActionResult OnGet()
        {
            ViewData["RemoteDeviceID"] = new SelectList(_context.RemoteDevices, "ID", "HostName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var RemoteDevice = await _context.RemoteDevices.Where(i => i.ID == RemoteDeviceID).FirstOrDefaultAsync();

            if (RemoteDevice != null)
            {
                _context.RemoteDevices.Remove(RemoteDevice);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
