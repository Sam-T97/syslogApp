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
    public class DeleteMailingListMemberModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        public string message;
        public DeleteMailingListMemberModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MailingListMember MailingListMember { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["MailingListID"] = new SelectList(_context.MailingLists, "ID", "ListName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["MailingListID"] = new SelectList(_context.MailingLists, "ID", "ListName");
            int id = await _context.MailingListMembers.Where(i => i.MailingListID == MailingListMember.MailingListID
                                                            && i.Email.ToLower() == MailingListMember.Email.ToLower())
                .Select(i => i.ID)
                .FirstOrDefaultAsync();
            if (id == 0)
            {
                message = "No one with that email was found in the mailing list";
                return Page();
            }

            MailingListMember = await _context.MailingListMembers.FindAsync(id);
            //This should be impossible given the check above but who knows what users can pull off 
            if (MailingListMember != null)
            {
                _context.MailingListMembers.Remove(MailingListMember);
                await _context.SaveChangesAsync();
                message = "Email removed from mailing list";
            }

            return Page();
        }
    }
}
