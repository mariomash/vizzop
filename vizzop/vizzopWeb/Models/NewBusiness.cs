using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Newbusiness
    {

        [Display(Name = "Client ID")]
        public int ID { get; set; }

        [Display(Name = "API key")]
        [StringLength(450)]
        public string ApiKey { get; set; }

        [Required]
        [Display(Name = "Your full name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [Required]
        [Display(Name = "Domain")]
        [MaxLength(15), MinLength(5)]
        //[RegularExpression(@"^\S+$", ErrorMessage = "No Spaces allowed")]
        [RegularExpression(@"^[a-zA-Z\d]+$", ErrorMessage = "Only Letters and numbers allowed")]
        public string Domain { get; set; }

        [Required]
        [Display(Name = "Your e-mail")]
        [Email(ErrorMessage = "Please write a valid E-mail")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Z\d]+$", ErrorMessage = "Only Letters and numbers allowed")]
        [Display(Name = "Administrator username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Choose a Password")]
        [MaxLength(15), MinLength(5)]
        /*
        [RegularExpression(@"^[a-zA-Z\d\x21\x23\x24\x26\x28\x29\x2A\x2B\x2D\x2E\x2C\x3F\x40\x5B\x5D\x5F\x7C\x7B\x7D]+$", ErrorMessage = "Only Letters,numbers and characters ! # $ & ) ( * + - . , ? @ ] [ _ | } { are allowed")]
         */
        public string Password { get; set; }

        [Display(Name = "VAT Number")]
        public string VAT { get; set; }

        [Display(Name = "Address")]
        [DataType(DataType.MultilineText)]
        [Column(TypeName = "text")]
        [AllowHtml]
        [MaxLength]
        public string Address { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "City or Town")]
        public string City { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [EqualTo("Password", ErrorMessage = "Please repeat Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public string Url { get; set; }

        [Display(Name = "Service Option")]
        public string ServiceType { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }

        [Display(Name = "Background color")]
        public string WidgetBackgroundColor { get; set; }

        [Display(Name = "Text color")]
        public string WidgetForegroundColor { get; set; }

        [Display(Name = "Border color")]
        public string WidgetBorderColor { get; set; }

        [Display(Name = "Text")]
        public string WidgetText { get; set; }

        [Display(Name = "Business Hours")]
        public string BusinessHours { get; set; }

    }
}