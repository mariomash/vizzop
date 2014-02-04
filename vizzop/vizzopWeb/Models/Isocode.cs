using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Isocode
    {
        public int ID { get; set; }

        [StringLength(10)]
        public string IsoCode { get; set; }

        public string Name { get; set; }

    }
}