using System;
using System.ComponentModel.DataAnnotations;
namespace SyslogShared.Models

{
    public class Alerts {
        [Key]
        public int ID { get; set; }
        public string Facility { get; set; }

        public DateTime Received { get; set; }

        public string HostIP { get; set; }

        public int Severity { get; set; }

        public string Message { get; set; }

        public bool Unread { get; set; }

        public int DeviceID { get; set; }
        public Device Device { get; set; }
    }
}