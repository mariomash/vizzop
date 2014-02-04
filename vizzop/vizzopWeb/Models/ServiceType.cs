using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ServiceType
    {
        public ServiceType()
        {
            this.CreatedOn = DateTime.Now;
        }

        [Display(Name = "ServiceType ID")]
        public int ID { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(450)]
        public string Name { get; set; }

        [Display(Name = "Cart Name (ID at 2Checkout)")]
        [StringLength(450)]
        public string CartName { get; set; }

        [Display(Name = "Cart Price")]
        public string Price { get; set; }

        [Display(Name = "Item Type")]
        public string Type { get; set; }


        [Required]
        [Display(Name = "ZIndex")]
        public int ZIndex { get; set; }


        [Required]
        public bool Active { get; set; }

    }
}