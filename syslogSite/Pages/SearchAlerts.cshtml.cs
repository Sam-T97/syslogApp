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
    public class SearchAlertsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public SearchAlertsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }
        public List<Alerts> Alerts { get;set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                Alerts = await _context.alerts
                    .Include(a => a.Device)
                    .Where(a => a.Device.HostName.Contains(SearchString)
                                || a.Facility.Contains(SearchString.ToUpper())
                                || a.HostIP == SearchString
                                || a.Message.Contains(SearchString))
                                .ToListAsync();


            }
            else
            {
                Alerts = await _context.alerts
                    .Include(d => d.Device)
                    .ToListAsync();
            }
        }
    }
}
