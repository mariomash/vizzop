using System;
using System.Collections.Generic;

namespace vizzopWeb.Models
{
    [Serializable]
    public class MeetingSession
    {

        public MeetingSession()
        {
            this.CreatedOn = DateTime.Now;
            this.Messages = new List<Message>();
            this.Conversers = new List<Converser>();
        }

        public int ID { get; set; }

        public int Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public string OpenTokSessionID { get; set; }

        public bool Public { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string SessionType { get; set; }

        public string Comments { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        public virtual Business Business { get; set; }

        public virtual ICollection<Converser> Conversers { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual Converser AssociatedConverser { get; set; }

    }
}