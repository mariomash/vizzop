using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multiply
{
    public enum OutputOption
    {
        console = 0,
        csv = 1,
        html = 2
    }

    class Program
    {

        static void Main(string[] args)
        {
            Utils utils = new Utils();

            const int MINIMUMNUMBER = 1;
            const int MAXIMUMNUMBER = 20;

            try
            {
                /*
                 * These are the default parameters
                 */
                int rows = 0;
                int cols = 0;
                OutputOption selectedoutput = OutputOption.console;

                /*
                 * Now we check all params passed by console...
                 * If there is some error we exit the application informing...
                 */
                if (args == null)
                {
                    utils.PrintError("We need at least one param");
                }
                else
                {
                    if (args.Length > 0)
                    {
                        if (Int32.TryParse(args[0], out rows) == false)
                        {
                            utils.PrintErrorAndExit("Rows param must be a number");
                        }
                        if (rows > MAXIMUMNUMBER || rows < MINIMUMNUMBER)
                        {
                            utils.PrintErrorAndExit(String.Format("Rows param must be a number between {0} and {1}", MINIMUMNUMBER, MAXIMUMNUMBER));
                        }
                        cols = rows;

                        if (args.Length > 1)
                        {
                            if (Int32.TryParse(args[1], out cols) == false)
                            {
                                if (args[1].ToString().ToUpperInvariant() == "HTML")
                                {
                                    selectedoutput = OutputOption.html;
                                }
                                else if (args[1].ToString().ToUpperInvariant() == "CSV")
                                {
                                    selectedoutput = OutputOption.csv;
                                }
                                else
                                {
                                    utils.PrintErrorAndExit("columns param must be a number");
                                }
                            }
                            if (cols > MAXIMUMNUMBER)
                            {
                                utils.PrintErrorAndExit(String.Format("Columns param must be a number between {0} and {1}", MINIMUMNUMBER, MAXIMUMNUMBER));
                            }

                            if (selectedoutput == OutputOption.console && args.Length > 2)
                            {
                                switch (args[2].ToString().ToUpperInvariant())
                                {
                                    case "HTML":
                                        selectedoutput = OutputOption.html;
                                        break;
                                    case "CSV":
                                        selectedoutput = OutputOption.csv;
                                        break;
                                    default:
                                        utils.PrintErrorAndExit("Third param must be CSV or HTML");
                                        break;
                                }
                                if (args.Length > 3)
                                {
                                    utils.PrintErrorAndExit("Too Much parameters");
                                }
                            }
                            else
                            {
                                utils.PrintErrorAndExit("Output param is repeated");
                            }
                        }
                    }
                    else
                    {
                        utils.PrintErrorAndExit("We need at least one param");
                    }

                    /*
                     * First we create the Multidimensional array with the table, so we can reuse the results...
                     */
                    MultiplicationTableCreator tablecreator = new MultiplicationTableCreator(rows, cols);
                    int[][] table = tablecreator.CreateTable();

                    /*
                     * Then we create the file we will save or return depending on the selected option...
                     */
                    MultiplicationOutputCreator outputcreador = new MultiplicationOutputCreator(table, selectedoutput);
                    string result = outputcreador.CreateOutput();

                }
            }
            catch (Exception ex)
            {
                utils.PrintErrorAndExit(ex.Message);
            }
        }
    }
}
