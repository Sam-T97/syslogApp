using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class UnreadAlertsDeviceFilteredModel : PageModel
    {
        public static string Hostname;
        private static int Hostid;
        private readonly SyslogShared.ApplicationDbContext _context;

        public UnreadAlertsDeviceFilteredModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Alerts> Alerts { get;set; }

        public async Task OnGetAsync(int id, string host)
        {
            Alerts = await _context.alerts
                .Where(a => a.Unread && a.DeviceID == id).ToListAsync();
            Hostname = host;
            Hostid = id;
        }
        public IActionResult OnGetDelete(int id)
        {
            var alert = _context.alerts.Single(i => i.ID == id);
            if (alert == null) return RedirectToAction("/UnreadAlertsDeviceFiltered");
            _context.alerts.Remove(alert);
            _context.SaveChanges();
            return RedirectToAction("/UnreadAlertsDeviceFiltered");
        }

        public IActionResult OnGetRead(int id, string host)
        {
            var alert = _context.alerts.Single(i => i.ID == id);
            if (alert == null) return RedirectToAction("/UnreadAlertsDeviceFiltered");
            alert.Unread = false;
            _context.SaveChanges();
            return RedirectToAction("/UnreadAlertsDeviceFiltered",routeValues: new {id = Hostid, host});
        }
    }
}
