using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Faq
    {
        public Faq()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            this.TimeStamp = localZone.ToUniversalTime(DateTime.Now);
            this.FaqDetails = new List<FaqDetails>();
        }

        [Key]
        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        public virtual Business Business { get; set; }

        public virtual ICollection<FaqDetails> FaqDetails { get; set; }

    }
}