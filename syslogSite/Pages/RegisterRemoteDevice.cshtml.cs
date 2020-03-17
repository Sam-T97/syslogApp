using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using SyslogShared;
using SyslogShared.Models;
using System.Threading;
using System.Xml.Linq;
using System.Web;
using System.Xml.XPath;
using Microsoft.AspNetCore.Hosting;

namespace syslogSite.Pages
{
    public class RegisterRemoteDeviceModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!£$%&";
        private IHostingEnvironment _env;
        public RegisterRemoteDeviceModel(SyslogShared.ApplicationDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }


        public IActionResult OnGet()
        {
            ViewData["DeviceID"] = new SelectList(_context.Devices.Where(i => i.RemoteDevice == null), "ID", "HostName");
            return Page();
        }

        [BindProperty]
        public int Device { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            RemoteDevice toAdd = new RemoteDevice();
            toAdd.HostName = await _context.Devices.Where(i => i.ID == Device)
                                        .Select(h => h.HostName).FirstOrDefaultAsync() + "PI";
            toAdd.Device = await _context.Devices.Where(i => i.ID == Device).FirstOrDefaultAsync();
            toAdd.DeviceID = Device;
            SshClient adClient = new SshClient("10.0.10.4", "website", "Password123!");
            adClient.Connect();
            ShellStream stream = adClient.CreateShellStream("", 20, 50, 1024, 1024, 500);
            stream.Flush();
            stream.WriteLine("powershell C:\\Users\\website\\CreatePI.ps1 \""+toAdd.HostName+"\" \"Password123!\"");
            Thread.Sleep(4000);
            adClient.Disconnect();
            adClient.Dispose();
            _context.RemoteDevices.Add(toAdd);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        public static string GetRandomPassword(int length)
        {
            var random = new Random();

            var pwd = "";

            for (var i = 0; i < length; i++)
            {
                pwd += Characters[random.Next(Characters.Length)];
            }

            return pwd;
        }
    }
}
