﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class DeviceAlertsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        public DeviceAlertsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Device> Device { get;set; }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices.Include(a => a.Alerts)
                .Where(a => a.Alerts.Count != 0).ToListAsync();
            
        }
        public IActionResult OnGetDeleteAll(int id)
        {
            var alert = _context.alerts.Where(i => i.DeviceID == id);
            if (!alert.Any()) return RedirectToAction("/DeviceUnreadAlerts");

            foreach (var item in alert)
            {
                _context.Remove(alert);
            }
            _context.SaveChanges();
            return RedirectToAction("/DeviceUnreadAlerts");
        }

        public IActionResult OnGetClearAll(int id)
        {
            var alert = _context.alerts.Where(i => i.DeviceID == id && i.Unread);
            if (!alert.Any()) return RedirectToAction("/DeviceUnreadAlerts"); 

            foreach (var item in alert)
            {
                item.Unread = false;
            }
            _context.SaveChanges();
            return RedirectToAction("/DeviceUnreadAlerts");
        }
    }
}
