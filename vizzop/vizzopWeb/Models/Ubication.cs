using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Ubication
    {
        [Display(Name = "Ubication ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public float Latitude { get; set; }
        public float longitude { get; set; }

    }
}