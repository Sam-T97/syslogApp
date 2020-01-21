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

namespace syslogSite.Pages
{
    public class IndexModel : PageModel
    {
        private ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
             
        }
    }
}
