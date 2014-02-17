using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class BrowserFeature
    {
        public BrowserFeature()
        {
            this.TimeStamp = DateTime.Now;
        }

        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        [StringLength(100)]
        public string UserAgent { get; set; }

        public Boolean AllowChatSockets { get; set; }

        public Boolean AllowScreenSockets { get; set; }

        public Boolean AllowScreenCaptures { get; set; }
    }
}