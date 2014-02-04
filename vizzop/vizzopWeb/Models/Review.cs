using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Review
    {
        public Review()
        {
            this.CreatedOn = DateTime.Now;
            this.Answered = false;
        }

        [Display(Name = "Review ID")]
        public int ID { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        public virtual Converser Converser { get; set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string Comment { get; set; }

        [Display(Name = "Review ID")]
        public int Rating { get; set; }

        public bool Answered { get; set; }
    }
}