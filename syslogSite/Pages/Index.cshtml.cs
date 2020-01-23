using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders.Testing;
using SyslogShared.Models;
using SyslogShared;
using syslogSite.Data;
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
