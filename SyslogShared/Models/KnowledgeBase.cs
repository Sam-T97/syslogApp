using System.ComponentModel.DataAnnotations;

namespace SyslogShared.Models
{
    public class KnowledgeBase
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Article { get; set; }
    }
}
