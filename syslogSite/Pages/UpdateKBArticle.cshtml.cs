using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyslogShared;
using SyslogShared.Models;

namespace syslogSite.Pages
{
    public class UpdateKBArticleModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public UpdateKBArticleModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public KnowledgeBase KnowledgeBase { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            KnowledgeBase = await _context.KnowledgeBases.FirstOrDefaultAsync(m => m.ID == id);

            if (KnowledgeBase == null)
            {
                return RedirectToAction("Index");
            }
            return Page();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(KnowledgeBase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KnowledgeBaseExists(KnowledgeBase.ID))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool KnowledgeBaseExists(int id)
        {
            return _context.KnowledgeBases.Any(e => e.ID == id);
        }
    }
}
