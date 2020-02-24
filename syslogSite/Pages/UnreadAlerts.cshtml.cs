using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class UnreadAlertsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public UnreadAlertsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Alerts> Alerts { get;set; }

        public async Task OnGetAsync()
        {
            Alerts = await _context.alerts
                .Include(a => a.Device).Where(a => a.Unread == true).ToListAsync();
        }

        public IActionResult OnGetDelete(int id)
        {
            var alert = _context.alerts.Single(i => i.ID == id);
            if (alert == null) return RedirectToAction("/UnreadAlerts");
            _context.alerts.Remove(alert);
            _context.SaveChanges();
            return RedirectToAction("/UnreadAlerts");
        }

        public IActionResult OnGetRead(int id)
        {
            var alert = _context.alerts.Single(i => i.ID == id);
            if (alert == null) return RedirectToAction("/UnreadAlerts");
            alert.Unread = false;
            _context.SaveChanges();
            return RedirectToAction("/UnreadAlerts");
        }
    }
}
