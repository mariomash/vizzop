using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ZenSession
    {
        public int ID { get; set; }

        public string sessionID { get; set; }

        public Converser Converser { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}