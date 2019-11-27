using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace syslogSite.Data.Models

{
    public class SyslogMessage {
        public int ID { get; set; }
        
        public string message { get; set; }
    }
}