using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class NewReview
    {
        [Required]
        public string Comment { get; set; }

        [Required]
        public int Rating { get; set; }

    }
}