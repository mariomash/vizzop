using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Business
    {
        public Business()
        {
            this.Active = false;
            this.CreatedOn = DateTime.Now;
            this.HideHelpOnStartup = false;
            this.ShowHelpButton = true;
            this.AuditMessages = false;
            this.AllowScreenCaptures = true;
            this.AllowSockets = true;
            this.ShowDisclaimer = false;
            this.AllowCaptureMouse = true;
            this.WidgetBackgroundColor = null;
            this.WidgetBorderColor = null;
            this.WidgetForegroundColor = null;
        }

        [Key]
        [Display(Name = "Client ID")]
        public int ID { get; set; }

        public bool Active { get; set; }

        [Display(Name = "API key")]
        [StringLength(450)]
        public string ApiKey { get; set; }

        [Required]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [Required]
        [Display(Name = "Domain")]
        [MaxLength(100), MinLength(5)]
        public string Domain { get; set; }

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
        [Display(Name = "E-mail")]
        [Email(ErrorMessage = "Please write a valid E-mail")]
        [StringLength(450)]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Business URL")]
        public string Url { get; set; }

        public string Comments { get; set; }

        [Display(Name = "Free, Standard or Premium")]
        public string AccountType { get; set; }

        [Required]
        [Display(Name = "Account type")]
        public string ServiceType { get; set; }

        [Required]
        [Display(Name = "Max. number of Agents")]
        public int MaxAgents { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }

        public bool ShowHelpButton { get; set; }

        public bool AuditMessages { get; set; }
        public bool AllowSockets { get; set; }
        public bool AllowScreenCaptures { get; set; }
        public bool ShowDisclaimer { get; set; }
        public bool AllowCaptureMouse { get; set; }


        public bool HideHelpOnStartup { get; set; }

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

        public virtual ICollection<Converser> Conversers { get; set; }

        public virtual ICollection<CommSession> CommSessions { get; set; }

    }
}