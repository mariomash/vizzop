using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Converser
    {

        public Converser()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            this.LastActive = localZone.ToUniversalTime(DateTime.Now);
            this.CreatedOn = localZone.ToUniversalTime(DateTime.Now);
            this.Password = Guid.NewGuid().ToString();
            this.Active = false;
            /*
            this.Business = new Business();
            this.Agent = new Agent();
            this.CommSessions = new List<CommSession>();
             * */
        }

        [Key]
        [Display(Name = "Converser ID")]
        public int ID { get; set; }

        public string FullName { get; set; }

        [Required]
        [StringLength(450)]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [StringLength(450)]
        public string Email { get; set; }

        [StringLength(450)]
        public string PhoneNumber { get; set; }

        [StringLength(450)]
        public string FacebookID { get; set; }

        [StringLength(450)]
        public string TwitterID { get; set; }

        [StringLength(10)]
        public string LangISO { get; set; }

        public string IP { get; set; }

        public string UserAgent { get; set; }

        //Tiene un agente??
        public virtual Agent Agent { get; set; }

        [Display(Name = "Last Active")]
        public DateTime LastActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual ICollection<CommSession> CommSessions { get; set; }

        /*
        [ForeignKey("ID")]
         */
        public virtual Business Business { get; set; }

        public bool Active { get; set; }

    }
}