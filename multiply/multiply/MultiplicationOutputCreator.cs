using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace multiply
{
    class MultiplicationOutputCreator
    {

        public int[][] Table { get; set; }
        public OutputOption SelectedOutput { get; set; }

        public MultiplicationOutputCreator(int[][] table, OutputOption selectedoutput)
        {
            this.Table = table;
            this.SelectedOutput = selectedoutput;
        }

        internal string CreateOutput()
        {
            throw new NotImplementedException();
        }
    }
}
