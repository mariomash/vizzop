using System;
using System.ComponentModel.DataAnnotations;


namespace vizzopWeb.Models
{
    [Serializable]
    public class FaqDetails
    {
        public FaqDetails()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            this.TimeStamp = localZone.ToUniversalTime(DateTime.Now);
        }

        /*[ForeignKey("ID")]*/
        public virtual Faq Faq { get; set; }

        [Key]
        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        public string LangISOCode { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }


    }
}