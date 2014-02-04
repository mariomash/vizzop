using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class LoginAction
    {
        public LoginAction()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            this.TimeStamp = localZone.ToUniversalTime(DateTime.Now);
        }

        public int ID { get; set; }

        public virtual Converser Converser { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}