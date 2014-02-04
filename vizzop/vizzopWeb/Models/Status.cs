using System;

namespace vizzopWeb.Models
{

    [Serializable]
    public class Status
    {
        public Boolean Success { get; set; }
        public Object Value { get; set; }

        public Status(Boolean success, Object value)
        {
            this.Success = success;
            this.Value = value;
        }
    }

}