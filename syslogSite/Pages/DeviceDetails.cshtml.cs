using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SyslogShared;
using SyslogShared.Models;
using Renci.SshNet;

namespace syslogSite.Pages
{
    public class DeviceDetailsModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private static SshClient terminalClient;

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

        public ActionResult OnGetBackupConfigs(int id)
        {
            try
            {
                string remoteIP;
                try
                {
                    remoteIP = _context.RemoteDevices.Where(i => i.DeviceID == id).Select(i => i.IP).First();
                }
                catch (Exception e)
                {
                    string[] error = {"No remote device configured"};
                    return new JsonResult(error);
                }
                SshClient client = new SshClient(remoteIP, "pi", "test");
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
                //Return failure if we can't connect to the PI
                return NotFound();
            }
        }
        public JsonResult OnGetRunningConfig(int id)
        {
            //try to grab the running config from the device
            try
            {
                string ip = _context.Devices.Where(i => i.ID == id).Select(i => i.IP).First();
                SshClient client = new SshClient(ip, "test", "test");
                client.Connect();
                SshCommand cmd = client.CreateCommand("show running-config");
                cmd.Execute();
                string config = cmd.Result;
                client.Disconnect();
                client.Dispose();
                return new JsonResult(config);
            }
            catch (Exception e)
            {
                //Return the SSH client message if it fails to connect 
                return new JsonResult(e.Message);
            }
        }

        public JsonResult OnGetInterfaceStatus(int id)
        {
            try
            {
                string ip = _context.Devices.Where(i => i.ID == id).Select(i => i.IP).First();
                SshClient client = new SshClient(ip, "test", "test");
                client.Connect();
                SshCommand cmd = client.CreateCommand("sh ip int br");
                cmd.Execute();
                string[] intDetails = {cmd.Result};
                client.Disconnect();
                client.Dispose();
                return new JsonResult(intDetails);
            }
            catch (Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet]
        public ActionResult OnGetCommand(string command)
        {
            try
            {
                GetClient();
                var cmd = terminalClient.RunCommand(command);
                return new JsonResult(new
                {
                    result = cmd.Result
                });
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        private void GetClient()
        {
            if (_cache.Get("client") != null)
            {
                terminalClient = (SshClient)_cache.Get("client");
            }
            else
            {
                terminalClient = new SshClient("test","test", "test"); //TODO get pi details from the DB and windows server
                terminalClient.Connect();
                _cache.Set("client", terminalClient); //TODO set this to a unique client identifier
            }
        }
    }
}
