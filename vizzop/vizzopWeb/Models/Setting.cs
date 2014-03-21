using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Setting
    {
        [Display(Name = "Setting ID")]
        public int ID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "Text")]
        public string Value { get; set; }

    }
}