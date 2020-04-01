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
    public class KnowledgeBaseModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;

        public KnowledgeBaseModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<KnowledgeBase> KnowledgeBase { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public async Task OnGetAsync()
        {
            if (!String.IsNullOrWhiteSpace(SearchString))
            {
                KnowledgeBase = await _context.KnowledgeBases
                    .Where(t => t.Title.ToUpper().Contains(SearchString.ToUpper()))
                    .Take(30)
                    .ToListAsync();
            }
            else
            {
                KnowledgeBase = await _context.KnowledgeBases
                    .Take(30)
                    .ToListAsync();
            }
        }

        public async Task OnGetDeleteAsync(int id)
        {
            if(id == 0)
            {
                KnowledgeBase = await _context.KnowledgeBases
                    .Take(30)
                    .ToListAsync();
            }
            else
            {
                var KBtoRemove = await _context.KnowledgeBases
                    .Where(i => i.ID == id).FirstOrDefaultAsync();
                _context.KnowledgeBases.Remove(KBtoRemove);
                await _context.SaveChangesAsync();
                KnowledgeBase = await _context.KnowledgeBases
                    .Take(30)
                    .ToListAsync();
            }
        }
    }
}

