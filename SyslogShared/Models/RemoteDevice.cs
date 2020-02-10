using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace SyslogShared.Models
{
    public class RemoteDevice
    {
        [Key]
        public int ID { get; set;}
        public string IP { get; set; }

        public string HostName { get; set; }
        public int DeviceID { get; set; }
        public Device Device { get; set; }
    }
}
