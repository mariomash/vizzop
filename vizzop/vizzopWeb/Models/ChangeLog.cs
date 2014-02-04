using System;
using System.ComponentModel.DataAnnotations;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ChangeLog
    {
        [NonSerialized]
        public vizzopContext db = new vizzopContext();

        [NonSerialized]
        public Utils utils = new Utils();

        public ChangeLog()
        {
            this.TimeStamp = DateTime.Now;
        }

        public Boolean AddToDb()
        {
            try
            {
                if (this.db == null)
                {
                    this.db = new vizzopContext();
                }
                db.ChangeLogs.Add(this);
                db.SaveChanges();

                return true;

            }
            catch (Exception ex)
            {
                try
                {
                    utils.GrabaLog(Utils.NivelLog.error, "Error Adding to DB: " + this.title + " " + this.Description);
                }
                catch (Exception ex_) { utils.GrabaLogExcepcion(ex_); }
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        [Display(Name = "Changelog ID")]
        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        [StringLength(50)]
        public string title { get; set; }

        public string Description { get; set; }
    }
}