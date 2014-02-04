using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ScreenMovie
    {

        [NonSerialized]
        public Utils utils = new Utils();

        public int ID { get; set; }

        [Required]
        public virtual Converser converser { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime ModifiedOn { get; set; }

        [Required]
        public DateTime LastFrameCreatedOn { get; set; }

        [Required]
        [Column(TypeName = "ntext")]
        [MaxLength]
        public string LastFrameData { get; set; }

        [Required]
        public int LastFrameWidth { get; set; }

        [Required]
        public int LastFrameHeight { get; set; }

        [Required]
        public int LastFrameScrollLeft { get; set; }

        [Required]
        public int LastFrameScrollTop { get; set; }

        [Required]
        public string LastFrameUrl { get; set; }

    }
}