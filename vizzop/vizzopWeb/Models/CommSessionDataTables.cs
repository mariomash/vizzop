using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class CommSessionDataTables
    {
        public int ID { get; set; }

        public string Status { get; set; }

        public string ClientName { get; set; }

        public string Agents { get; set; }

        public string Comments { get; set; }

        public string WaitingFor { get; set; }
        public DateTime LastAlive { get; set; }

        public string SessionType { get; set; }

        public string LastMessage { get; set; }

        public string LockedBy { get; set; }

        public string LastViewed { get; set; }

        public bool LastMsgIsFromClient { get; set; }

        public int NumberOfMessages { get; set; }

    }
}