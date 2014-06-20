using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multiply
{
    class Utils
    {
        public void PrintErrorAndExit(string ErrorDescription)
        {
            PrintError(ErrorDescription);
            //System.Environment.Exit(0);
            Console.ReadLine();
        }

        public void PrintError(string ErrorDescription)
        {
            string UsageMsg = "multiply.exe rows [cols] [output-format: CSV|HTML]";
            string OutputMsg = string.Format("USAGE: {0}\n", UsageMsg);

            if (ErrorDescription != null)
            {
                OutputMsg = string.Format("ERROR: {0}\n{1}\n", ErrorDescription.ToString(), OutputMsg);
            }

            Console.Write(OutputMsg);
        }
    }
}
