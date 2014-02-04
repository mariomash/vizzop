using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class Agent
    {
        public Agent()
        {
            this.IsAdmin = false;
            this.CreatedOn = DateTime.Now;
            this.LastLogged = DateTime.Now;
        }

        [Display(Name = "Agent ID")]
        public int ID { get; set; }

        public DateTime LastLogged { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool Active { get; set; }

        public bool IsAdmin { get; set; }

        public virtual ICollection<Converser> Clients { get; set; }

        public virtual Converser Converser { get; set; }

        public virtual ICollection<CommSession> CommSessions { get; set; }

    }
}