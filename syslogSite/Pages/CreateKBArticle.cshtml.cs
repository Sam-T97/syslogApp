using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class CreateKBArticleModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public CreateKBArticleModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public KnowledgeBase KnowledgeBase { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.KnowledgeBases.Add(KnowledgeBase);
            await _context.SaveChangesAsync();

            return RedirectToPage("./KnowledgeBase");
        }
    }
}
