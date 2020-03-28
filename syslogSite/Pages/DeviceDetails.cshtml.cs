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
        private static SshClient _terminalClient;

        public DeviceDetailsModel(SyslogShared.ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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
                string piIP;
                try
                {
                    string piHost = _context.RemoteDevices.Where(d => d.Device.ID == id).Select(h => h.HostName).First();
                    using SshClient adClient = new SshClient("10.0.10.4","website","Password123!");
                    adClient.Connect();
                    var adCMD = adClient.CreateCommand("powershell C:\\Users\\website\\GetClients.ps1 \"" + piHost + "\"");
                    adCMD.Execute();
                    piIP = adCMD.Result;
                    piIP = piIP.TrimEnd('\n');
                    adClient.Disconnect();
                    adClient.Dispose();
                }
                catch
                {
                    string[] error = {"No remote device configured"};
                    return new JsonResult(error);
                }
                SshClient client = new SshClient(piIP, "pi", "test");
                client.Connect();
                SshCommand cmd = client.CreateCommand("./ListConfigs.py");
                cmd.Execute();
                if (String.IsNullOrWhiteSpace(cmd.Result))
                {
                    return new JsonResult(new EmptyResult());
                }
                List<string> configs = cmd.Result.Split(new[] {"\n"}, StringSplitOptions.None).ToList();
                client.Disconnect();
                client.Dispose();
                return new JsonResult(configs);
            }
            catch
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
        public ActionResult OnGetCommand(string command, int id)
        {
            try
            {
                GetClient(id);
                dynamic cmd = !string.IsNullOrWhiteSpace(command) ? _terminalClient.RunCommand("./SendCommand.py " + "\"" + command + "\"") 
                    : _terminalClient.RunCommand("./SendCommand.py");
                return new JsonResult(new
                {
                    result = cmd.Result
                });
            }
            catch
            {
                return NotFound();
            }
        }

        private void GetClient(int id)
        {
            if (_cache.Get("client" + id) != null)
            {
                _terminalClient = (SshClient)_cache.Get("client"+id);
            }
            else
            {
                string piIP;
                try
                {
                    string piHost = _context.RemoteDevices.Where(d => d.Device.ID == id).Select(h => h.HostName).First();
                    using SshClient adClient = new SshClient("10.0.10.4", "website", "Password123!");
                    adClient.Connect();
                    var adCMD = adClient.CreateCommand("powershell C:\\Users\\website\\GetClients.ps1 \"" + piHost + "\"");
                    adCMD.Execute();
                    piIP = adCMD.Result;
                    piIP = piIP.TrimEnd('\n');
                    adClient.Disconnect();
                    adClient.Dispose();
                    _terminalClient = new SshClient(piIP, "pi", "test");
                    _terminalClient.Connect();
                    _cache.Set("client"+ id, _terminalClient); //TODO set this to a unique client identifier
                }
                catch
                {
                    _cache.Set("client"+id, _terminalClient); //Set null client to cache to trigger error message
                }
            }
        }

        public ActionResult OnGetViewBackupConfig(int id, string config)
        {
            try
            {
                GetClient(id);
                var cmd = _terminalClient.RunCommand("cat Configs/config" + config);
                return new JsonResult(cmd.Result);
            }
            catch
            {
                return new JsonResult("We had some trouble getting the backup configuration from the remote device");
            }
        }

        public ActionResult OnGetRollbackConfig(int id, string config)
        {
            try
            {
                GetClient(id);
                var cmd = _terminalClient.CreateCommand("./TestLogin.py && ./Restore.py " + config);
                return new JsonResult(new
                {
                    result = cmd.Result
                });
            }
            catch (Exception e)
            {
                return new JsonResult(new
                {
                    result = "Failed",
                    error = e.Message
                });
            }
        }
    }
}
