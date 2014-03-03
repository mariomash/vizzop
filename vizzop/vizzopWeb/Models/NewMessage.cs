using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{
    [Serializable]
    public class NewMessage
    {
        public NewMessage()
        {
            this.TimeStampSrvAccepted = DateTime.Now.ToUniversalTime();
        }

        [Required]
        public string From { get; set; }

        public string From_FullName { get; set; }

        [Required]
        public string To { get; set; }

        public string CC { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string _clientid { get; set; }

        public string _status { get; set; }

        public string TimeStamp { get; set; }

        public string TimeStampSenderSending { get; set; }

        public DateTime TimeStampSrvAccepted { get; set; }

        public string Lang { get; set; }

        public string MessageType { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; }

        public string commsessionid { get; set; }

        public vizzopContext db { get; set; }
    }
}