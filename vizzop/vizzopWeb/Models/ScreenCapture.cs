using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Threading.Tasks;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ScreenCapture
    {

        public ScreenCapture()
        {
            this.GUID = Guid.NewGuid().ToString();
            this.CreatedOn = DateTime.UtcNow;
            this.ReceivedOn = DateTime.UtcNow;
            this.PicturedOn = DateTime.UtcNow;
        }

        [NonSerialized]
        public Utils utils = new Utils();

        public int ID { get; set; }

        [Required]
        public virtual Converser converser { get; set; }

        public string WindowName { get; set; }

        /*[Required]*/
        [Column(TypeName = "ntext")]
        [MaxLength]
        public string Data { get; set; }

        [Column(TypeName = "ntext")]
        [MaxLength]
        public string Blob { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int ScrollLeft { get; set; }

        [Required]
        public int ScrollTop { get; set; }

        [Required]
        public int MouseX { get; set; }

        [Required]
        public int MouseY { get; set; }

        [StringLength(8000)]
        public string Headers { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime ReceivedOn { get; set; }

        public DateTime PicturedOn { get; set; }

        [Required]
        public string GUID { get; set; }

    }
}