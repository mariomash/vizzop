using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class CommSession
    {
        public CommSession()
        {
            this.CreatedOn = DateTime.Now;
            this.Messages = new List<Message>();
            this.Client = new Converser();
            this.Agents = new List<Agent>();
        }
        [Display(Name = "CommSession ID")]
        public int ID { get; set; }

        public virtual Converser LockedBy { get; set; }

        //Status Codes
        //0 == Pending Aproval
        //1 == Support approved, supporting Right Now
        //2 == Support Denied
        //3 == Support Ended
        public int Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public string SessionType { get; set; }

        public string Comments { get; set; }

        [Column(TypeName = "ntext")]
        public string OpenTokSessionID { get; set; }

        public virtual Business Business { get; set; }

        public virtual Converser Client { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}