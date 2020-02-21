using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SyslogShared.Models
{
    public class Device
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string HostName { get; set; }
        [Required]
        public string IP { get; set; }
        
        public RemoteDevice RemoteDevice { get; set; }
        public ICollection<Alerts> Alerts { get; set; }
    }
}
