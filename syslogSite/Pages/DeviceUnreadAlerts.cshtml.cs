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
    public class DeviceUnreadAlertsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public DeviceUnreadAlertsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Device> Device { get;set; }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices.Include(a => a.Alerts)
                .Where(a => a.Alerts.Count != 0).ToListAsync();
        }
    }
}
