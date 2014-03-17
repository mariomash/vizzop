using System;

namespace vizzopWeb.Models
{
    [Serializable]
    public class WebLocationDataTables : IEquatable<WebLocationDataTables>
    {
        public int ID { get; set; }

        public string Url { get; set; }

        public string Referrer { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public string Domain { get; set; }

        public string Password { get; set; }

        public DateTime TimeStamp { get; set; }

        public string LastViewed { get; set; }

        public string UserAgent { get; set; }

        public string Lang { get; set; }

        public string IP { get; set; }

        public string Ubication { get; set; }

        public string WindowName { get; set; }

        public bool Equals(WebLocationDataTables other)
        {

            //Check whether the compared object is null. 
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data. 
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal. 
            return ID.Equals(other.ID) && Url.Equals(other.Url) && Referrer.Equals(other.Referrer) && FullName.Equals(other.FullName) && UserName.Equals(other.UserName) && Domain.Equals(other.Domain) && Password.Equals(other.Password) && UserAgent.Equals(other.UserAgent) && Lang.Equals(other.Lang) && IP.Equals(other.IP) && Ubication.Equals(other.Ubication);

            //&& TimeStamp.Equals(other.TimeStamp) && LastViewed.Equals(other.LastViewed)
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 
        public override int GetHashCode()
        {

            int hID = ID.GetHashCode();
            int hUrl = Url == null ? 0 : Url.GetHashCode();
            int hReferrer = Referrer == null ? 0 : Referrer.GetHashCode();
            int hFullName = FullName == null ? 0 : FullName.GetHashCode();
            int hUserName = UserName == null ? 0 : UserName.GetHashCode();
            int hDomain = Domain == null ? 0 : Domain.GetHashCode();
            int hPassword = Password == null ? 0 : Password.GetHashCode();
            //int hTimeStamp = TimeStamp == null ? 0 : TimeStamp.GetHashCode();
            //int hLastViewed = LastViewed == null ? 0 : LastViewed.GetHashCode();
            int hUserAgent = UserAgent == null ? 0 : UserAgent.GetHashCode();
            int hLang = Lang == null ? 0 : Lang.GetHashCode();
            int hIP = IP == null ? 0 : IP.GetHashCode();
            int hUbication = Ubication == null ? 0 : Ubication.GetHashCode();

            //Calculate the hash code for the product. 
            return hID ^ hUrl ^ hReferrer ^ hFullName ^ hUserName ^ hDomain ^ hPassword ^ hUserAgent ^ hLang ^ hIP ^ hUbication;
            //^ hTimeStamp ^ hLastViewed
        }

    }
}