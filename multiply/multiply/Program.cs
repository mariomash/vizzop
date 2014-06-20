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

    class Program
    {
        public enum OutputOption
        {
            console = 0,
            csv = 1,
            html = 2
        }

        static void Main(string[] args)
        {
            Utils utils = new Utils();
            try
            {
                int firstnumber;
                int secondnumber;
                OutputOption outputoption = OutputOption.console;

                if (args == null)
                {
                    utils.PrintError("We need at least one param");
                }
                else
                {
                    if (args.Length > 0)
                    {
                        if (Int32.TryParse(args[0], out firstnumber) == false)
                        {
                            utils.PrintErrorAndExit(String.Format("First param must be a number smaller than {0}", Int32.MaxValue.ToString()));
                        }
                        if (args.Length > 1)
                        {
                            if (Int32.TryParse(args[1], out secondnumber) == false)
                            {
                                utils.PrintErrorAndExit(String.Format("Second param must be a number smaller than {0}", Int32.MaxValue.ToString()));
                            }

                            if (args.Length > 2)
                            {
                                if ((args[2].ToString().ToUpperInvariant() != "CSV") || (args[2].ToString().ToUpperInvariant() != "HTML"))
                                {
                                    utils.PrintErrorAndExit("Third param must be CSV or HTML");
                                }
                                else
                                {
                                    switch (args[2].ToString().ToUpperInvariant())
                                    {
                                        case "HTML":
                                            outputoption = OutputOption.html;
                                            break;
                                        case "CSV":
                                            outputoption = OutputOption.csv;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                if (args.Length > 3)
                                {
                                    utils.PrintErrorAndExit("Too Much parameters");
                                }
                            }
                        }
                    }
                    else
                    {
                        utils.PrintErrorAndExit("We need at least one param");
                    }
                }
            }
            catch (Exception ex)
            {
                utils.PrintErrorAndExit(ex.Message);
            }
        }
    }
}
