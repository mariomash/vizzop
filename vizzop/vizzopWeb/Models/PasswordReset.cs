using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace vizzopWeb.Models
{
    [Serializable]
    public class PasswordReset
    {

        public PasswordReset()
        {
            this.CreatedOn = DateTime.Now;
            this.Key = Guid.NewGuid().ToString();
        }

        [Display(Name = "Converser ID")]
        public int ID { get; set; }

        [Display(Name = "UserName")]
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        public virtual Business Business { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Choose a Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [EqualTo("Password", ErrorMessage = "Please repeat Password")]
        public string ConfirmPassword { get; set; }

        public string Key { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}