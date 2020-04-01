using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SyslogShared.Models
{
    public class MailingList
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string ListName { get; set; }

        public ICollection<MailingListMember> MailingListMembers { get; set; }
    }
}
