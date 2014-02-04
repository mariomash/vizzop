using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class TextString
    {
        [Display(Name = "TextString ID")]
        public int ID { get; set; }

        [StringLength(50)]
        public string Ref { get; set; }

        [StringLength(5)]
        public string IsoCode { get; set; }

        [Column(TypeName = "Text")]
        public string Text { get; set; }

    }
}