using System;
using System.ComponentModel.DataAnnotations;


namespace SyslogShared.Models
{
    public class AppVars
    {
        [Key]
        public int ID { get; set; }
        public string VariableName { get; set; }

        public string Value { get; set; }
    }
}
