using System.ComponentModel.DataAnnotations;

namespace SyslogShared.Models
{
    public class MailingListMember
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Email { get; set; }
        
        public int MailingListID { get; set; }
        public MailingList MailingList { get; set; }

    }
}
