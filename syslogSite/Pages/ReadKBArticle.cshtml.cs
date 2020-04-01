using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class ReadKBArticleModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public ReadKBArticleModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public KnowledgeBase KnowledgeBase { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            KnowledgeBase = await _context.KnowledgeBases.FirstOrDefaultAsync(m => m.ID == id);

            if (KnowledgeBase == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
