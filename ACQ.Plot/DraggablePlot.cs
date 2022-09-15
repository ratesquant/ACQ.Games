using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ACQ.Plot
{
    public class DraggablePlot1D : ScottPlot.Plottable.ScatterPlotListDraggable
    {
        public DataTable Data
        {
            get
            {
                DataTable dt = new DataTable();
                var colx =  dt.Columns.Add("X", typeof(double));
                var coly =  dt.Columns.Add("Y", typeof(double));

                for (int i = 0; i < this.Xs.Count; i++)
                {
                    dt.Rows.Add(this.Xs[i], this.Ys[i]);
                }
                return dt;
            }
        }
    }
}
