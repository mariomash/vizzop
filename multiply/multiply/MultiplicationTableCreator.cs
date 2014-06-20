using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace multiply
{
    class MultiplicationTableCreator
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int[][] Table { get; set; }

        public MultiplicationTableCreator(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
        }

        internal int[][] CreateTable()
        {
            return Table;
        }

    }
}
