using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizzopWeb.Models
{

    [Serializable]
    public class MessageAudit //: ISerializable
    {

        public MessageAudit()
        {
            this.TimeStamp = DateTime.UtcNow;
            this.TimeStampRecipientAccepted = DateTime.UtcNow;
            this.TimeStampSenderSending = DateTime.UtcNow;
            this.TimeStampSrvAccepted = DateTime.UtcNow;
            this.TimeStampSrvSending = DateTime.UtcNow;
        }

        [NonSerialized]
        public vizzopContext db = new vizzopContext();

        [NonSerialized]
        public Utils utils = new Utils();

        [Display(Name = "Message ID")]
        public int ID { get; set; }

        [Display(Name = "CreatedOn TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [Display(Name = "ClientSending TimeStamp")]
        public DateTime TimeStampSenderSending { get; set; }

        [Display(Name = "SrvAccepted TimeStamp")]
        public DateTime TimeStampSrvAccepted { get; set; }

        [Display(Name = "SrvSending TimeStamp")]
        public DateTime TimeStampSrvSending { get; set; }

        [Display(Name = "SrvAccepted TimeStamp")]
        public DateTime TimeStampRecipientAccepted { get; set; }

        public string MessageType { get; set; }

        //Cuando tengamos varios servers esto nos dice donde esta el cuello de botella...
        public string MainURL { get; set; }

        public virtual Converser From { get; set; }

        public virtual Converser To { get; set; }

        public string Subject { get; set; }

        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        public virtual CommSession CommSession { get; set; }

    }
}