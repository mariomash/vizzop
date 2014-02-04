using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace vizzopWeb.Models
{
    [Serializable]
    public class NewAgent
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = " E-mail")]
        [Email(ErrorMessage = "Please write a valid E-mail")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Choose a Password")]
        [MaxLength(15), MinLength(5)]
        /*
        [RegularExpression(@"^[a-zA-Z\d\x21\x23\x24\x26\x28\x29\x2A\x2B\x2D\x2E\x2C\x3F\x40\x5B\x5D\x5F\x7C\x7B\x7D]+$", ErrorMessage = "Only Letters,numbers and characters ! # $ & ) ( * + - . , ? @ ] [ _ | } { are allowed")]
         * */
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [EqualTo("Password", ErrorMessage = "Please repeat Password")]
        public string ConfirmPassword { get; set; }

        public bool Active { get; set; }

        public int ID { get; set; }
    }
}