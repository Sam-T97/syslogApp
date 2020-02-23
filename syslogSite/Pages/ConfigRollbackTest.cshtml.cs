using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Renci.SshNet;

namespace syslogSite.Pages
{
    public class ConfigRollbackTestModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public void OnGet()
        {

        }

        public JsonResult OnGetConfigs()
        {
            SshClient client = new SshClient("TestPI","pi","test");
            client.Connect();
            SshCommand cmd = client.CreateCommand("./ListConfigs.py");
            cmd.Execute();
            List<string> configs = cmd.Result.Split(new[] {"\n"}, StringSplitOptions.None).ToList();
            client.Disconnect();
            return new JsonResult(configs);
        }
    }
}