using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace vizzopWeb.Models
{
    [Serializable]
    public class NewvizzopAccount
    {

        public NewvizzopAccount()
        {
            this.AcceptTerms = true;
        }

        [Required]
        [Display(Name = "Your full name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Your business name")]
        public string BusinessName { get; set; }


        [Required]
        [Display(Name = "Your e-mail")]
        [Email(ErrorMessage = "Please write a valid E-mail")]
        public string Email { get; set; }

        [Display(Name = "Your phone number")]
        [Phone(ErrorMessage = "Please write a valid Phone number")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MaxLength(20), MinLength(5)]
        /*
        [RegularExpression(@"^[a-zA-Z\d]+$", ErrorMessage = "Only Letters and numbers allowed")]
         */
        /*
        [RegularExpression(@"^[a-zA-Z\d\x21\x23\x24\x26\x28\x29\x2A\x2B\x2D\x2E\x2C\x3F\x40\x5B\x5D\x5F\x7C\x7B\x7D]+$", ErrorMessage = "Only Letters,numbers and characters ! # $ & ) ( * + - . , ? @ ] [ _ | } { are allowed")]
         */
        [Display(Name = "Choose a password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Re-enter password")]
        [EqualTo("Password", ErrorMessage = "Please repeat Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Choose account type")]
        public string ServiceType { get; set; }

        [Display(Name = "I accept terms and conditions")]
        [BooleanRequired(ErrorMessage = "You must accept to continue")]
        public bool AcceptTerms { get; set; }
    }
}