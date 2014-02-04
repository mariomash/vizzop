using System;
using System.ComponentModel.DataAnnotations;


namespace vizzopWeb.Models
{
    [Serializable]
    public class Country
    {
        [Display(Name = "Country ID")]
        public int ID { get; set; }

        [StringLength(100)]
        public string CountryName { get; set; }

        [StringLength(2)]
        public string CountryCode { get; set; }

        [StringLength(10)]
        public string CountryPrefix { get; set; }
    }
}