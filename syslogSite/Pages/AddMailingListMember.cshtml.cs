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
    public class AddMailingListMemberModel : PageModel
    {
        private readonly SyslogShared.ApplicationDbContext _context;
        public string message;
        public AddMailingListMemberModel(SyslogShared.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["MailingListID"] = new SelectList(_context.MailingLists, "ID", "ListName");
            return Page();
        }

        [BindProperty]
        public MailingListMember MailingListMember { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["MailingListID"] = new SelectList(_context.MailingLists, "ID", "ListName");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var checkExists = _context.MailingListMembers.Where(i =>
                i.MailingListID == MailingListMember.MailingListID && i.Email.ToLower() == MailingListMember.Email.ToLower() );
            //Escape here if the user is already added 
            if (checkExists.Any())
            {
                message = "Email is already in this list"; 
                return Page();
            }
            _context.MailingListMembers.Add(MailingListMember);
            await _context.SaveChangesAsync();
            message = "Email added to mailing list";
            return Page();
        }

        public ActionResult OnGetMembers(int id)
        {
            List<string> memberList = _context.MailingListMembers
                .Where(i => i.MailingListID == id)
                .Select(e => e.Email).ToList();
            return new JsonResult(new{
                emails = memberList});
        }
    }
}
