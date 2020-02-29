using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders.Testing;
using SyslogShared.Models;
using SyslogShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Newtonsoft.Json;

namespace syslogSite.Pages
{
    public class IndexModel : PageModel
    {
        public string AlertTotal { get; set; }
        private ApplicationDbContext _context = GetContext();
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            AlertTotal = _context.alerts.Count().ToString();
        }

        public JsonResult OnGetUpdateMetrics()
        {
            List<string> ErrorNames = new List<string>
                {"Emergency", "Alert", "Critical", "Error", "Warning", "Notification", "Information", "Debug"};
            Dictionary<string,string> counts = new Dictionary<string, string>();
            for (int i = 0; i < 8; i++)
            {
                counts.Add(ErrorNames[i], _context.alerts.Count(a => a.Severity == i).ToString());
            }
            counts.Add("Total", _context.alerts.Count().ToString());
            return new JsonResult(JsonConvert.SerializeObject(counts));
        }

        public  JsonResult OnGetTroubleSystems()
        {
            List<string> devices = _context.Devices.Include(a => a.Alerts)
                .OrderByDescending(d => d.Alerts.Count)
                .Select(d => d.HostName)
                .Take(5).ToList();
            return new JsonResult(new {hostnames = devices});
        }

        public JsonResult OnGetInboundData(int time)
        {
            DateTime startDateTime;
            var result = (dynamic) null;
            switch (time)
            {
                case 0:
                    startDateTime = DateTime.Now - TimeSpan.FromHours(1);
                    result = _context.alerts
                        .Where(a => a.Received >= startDateTime && a.Received <= DateTime.Now)
                        .GroupBy(a => a.Received.Minute)
                        .Select(x => new
                        {
                            day = x.Key,
                            rate = x.Count()
                        });
                    break;
                case 1:
                    startDateTime = DateTime.Now - TimeSpan.FromDays(1);
                    result = _context.alerts
                        .Where(a => a.Received >= startDateTime && a.Received <= DateTime.Now)
                        .GroupBy(a => a.Received.Day)
                        .Select(x => new
                        {
                            day = x.Key,
                            rate = x.Count()
                        });
                    break;
                case 2:
                    startDateTime = DateTime.Now - TimeSpan.FromDays(7);
                    result = _context.alerts
                        .Where(a => a.Received >= startDateTime && a.Received <= DateTime.Now)
                        .GroupBy(a => a.Received.Day)
                        .Select(x => new
                        {
                            day = x.Key,
                            rate = x.Count()
                        });
                    break;
                case 3:
                    startDateTime = DateTime.Now - TimeSpan.FromDays(30);
                    result = _context.alerts
                            .Where(a => a.Received >= startDateTime && a.Received <= DateTime.Now)
                            .GroupBy(a => a.Received.Day)
                            .Select(x => new
                            {
                                day = x.Key,
                                rate = x.Count()
                            });
                    break;
            }

            return new JsonResult(result);
        }
        public static ApplicationDbContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(
                GetConfig().GetConnectionString("DefaultConnection"),
                mySqlOptions =>
                {
                    mySqlOptions.ServerVersion(new Version(8, 0, 17), ServerType.MySql);
                    mySqlOptions.MigrationsAssembly("syslogSite");
                });
            return new ApplicationDbContext(optionsBuilder.Options);
        }
        private static IConfiguration GetConfig()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
