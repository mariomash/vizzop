using Microsoft.Win32;
//using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace vizzopWeb
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            /*
            try
            {
                var IEVAlue = 11999; // 9000; // can be: 9999 , 9000, 8888, 8000, 7000
                var targetApplication = "w3wp.exe";
                var localMachine = Registry.LocalMachine;
                var KeyLocation = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
                //var keyName = "FEATURE_BROWSER_EMULATION";
                // "opening up Key: {0} at {1}".info(keyName, parentKeyLocation);
                var subKey = localMachine.CreateSubKey(KeyLocation);
                subKey.SetValue(targetApplication, IEVAlue, RegistryValueKind.DWord);
                //return "all done, now try it on a new process".info();
            }
            catch (System.Exception)
            {
                //ex.log();
                //"NOTE: you need to run this under no UAC".info();
            }
            */
            return base.OnStart();
        }
    }
}
