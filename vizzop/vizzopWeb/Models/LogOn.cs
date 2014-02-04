using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;


namespace vizzopWeb.Models
{
    [Serializable]
    public class LogOn
    {
        [Display(Name = "E-mail")]
        [Email(ErrorMessage = "Please write a valid E-mail")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}