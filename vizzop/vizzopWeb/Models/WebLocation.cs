using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class WebLocation
    {

        public WebLocation()
        {
            this.MustGenerateScreenshot = true;
            this.Guid = System.Guid.NewGuid().ToString();
        }

        /*
        public int ID { get; set; }
        */
        public string Guid { get; set; }
        /*
        [Required]
        public virtual Converser Converser { get; set; }
        */

        [Required]
        public int ConverserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Domain { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        //[Column(TypeName = "nvarchar(MAX)")]
        [StringLength(8000)]
        public string Url { get; set; }

        //[Column(TypeName = "nvarchar(MAX)")]
        [StringLength(8000)]
        public string Referrer { get; set; }

        //[Column(TypeName = "nvarchar(MAX)")]
        [StringLength(8000)]
        public string UserAgent { get; set; }

        [StringLength(8000)]
        public string Headers { get; set; }

        [StringLength(10)]
        public string Lang { get; set; }

        [StringLength(39)]
        public string IP { get; set; }

        public string Ubication { get; set; }

        [Required]
        public DateTime TimeStamp_First { get; set; }

        [Required]
        public DateTime TimeStamp_Last { get; set; }

        public string WindowName { get; set; }

        public bool MustGenerateScreenshot { get; set; }
        public string CaptureProcessId { get; set; }
        public ScreenCapture ScreenCapture { get; set; }
        public string ThumbNail { get; set; }
        public string CompleteHtml { get; set; }
    }
}