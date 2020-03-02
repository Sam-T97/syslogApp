using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;
using Renci.SshNet;

namespace syslogSite.Pages
{
    public class DeviceDetailsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public DeviceDetailsModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices.FirstOrDefaultAsync(m => m.ID == id);

            if (Device == null)
            {
                return NotFound();
            }
            return Page();
        }

        public ActionResult OnGetBackupConfigs()
        {
            try
            {
                SshClient client = new SshClient("TestPI", "pi", "test");
                client.Connect();
                SshCommand cmd = client.CreateCommand("./ListConfigs.py");
                cmd.Execute();
                List<string> configs = cmd.Result.Split(new[] {"\n"}, StringSplitOptions.None).ToList();
                client.Disconnect();
                client.Dispose();
                return new JsonResult(configs);
            }
            catch(Exception e)
            {
                return NotFound();
            }
        }
        public JsonResult OnGetRunningConfig()
        {
            try
            {
                SshClient client = new SshClient("10.0.10.1", "test", "test");
                client.Connect();
                SshCommand cmd = client.CreateCommand("sh run");
                cmd.Execute();
                string config = cmd.Result;
                client.Disconnect();
                client.Dispose();
                return new JsonResult(config);
            }
            catch (Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
    }
}
