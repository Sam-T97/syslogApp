using System.ComponentModel.DataAnnotations;
namespace SyslogShared.Models

{
    public class Alerts {
        [Key]
        public int ID { get; set; }
        
        public string message { get; set; }
    }
}