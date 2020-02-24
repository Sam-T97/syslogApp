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
    public class AllAlertsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public AllAlertsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Alerts> Alerts { get;set; }

        public async Task OnGetAsync()
        {
            Alerts = await _context.alerts.Include(p => p.Device).ToListAsync();
        }
    }
}
