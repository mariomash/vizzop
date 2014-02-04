using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace vizzopWeb.Models
{
    [Serializable]
    public class ScreenCaptureControl
    {
        public ScreenCaptureControl()
        {
            //this.ProcessID = null;
            //this.ScreenCapture = null;
            //this.LastHtml = null;
            //this.GUIDToProcess = null;
            //this.CreateProcess = false;
        }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ScreenCapture ScreenCapture { get; set; }


        public string CompleteHtml { get; set; }
    }
}