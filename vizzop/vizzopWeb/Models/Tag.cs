using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Tag
    {
        public Tag()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            this.TimeStamp = localZone.ToUniversalTime(DateTime.Now);
        }

        public int ID { get; set; }

        public string Text { get; set; }

        public DateTime TimeStamp { get; set; }

    }
}