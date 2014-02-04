using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Sale
    {
        public Sale()
        {
            this.TimeStamp = DateTime.Now;
            this.Payed = false;
        }
        [Display(Name = "TextString ID")]
        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        [StringLength(100)]
        public string Hash { get; set; }

        [StringLength(50)]
        public string Comments { get; set; }

        public virtual Business Business { get; set; }

        public string servicetype { get; set; }

        public Boolean Payed { get; set; }

        public string Total { get; set; }
    }
}