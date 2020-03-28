using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class RegisterDeviceModel : PageModel
    {
        public static string feedback;
        private readonly SyslogShared.ApplicationDbContext _context;

        public RegisterDeviceModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            feedback = "";
            return Page();
        }

        [BindProperty]
        public Device Device { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var check = _context.Devices.Any(i => i.IP == Device.IP || i.HostName == Device.HostName);
            if (check == true)
            {
                feedback = "A device already exists in the system with either: the same Hostname or IP address. \n" +
                           "Please check the devices in the system against the data you wish to register and try again.";
                return Page();
            }

            try
            {
                Ping testPing = new Ping();
                PingReply reply = testPing.Send(Device.IP, 2000);
                if (reply.Status != IPStatus.Success)
                {
                    feedback = "The server could not reach the IP address you specified. \n" +
                               "Make sure the device you're trying to add is alive and can reach this server";
                    testPing.Dispose();
                    return Page();
                }
                testPing.Dispose();
                _context.Devices.Add(Device);
                await _context.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                feedback = "An error occured trying to register the device \n" +
                           e.Message;
                return Page();
            }
            return RedirectToPage("./Index");
        }
    }
}
