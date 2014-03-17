using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class WebLocation
    {

        public int ID { get; set; }

        [Required]
        public virtual Converser Converser { get; set; }

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
    }
}