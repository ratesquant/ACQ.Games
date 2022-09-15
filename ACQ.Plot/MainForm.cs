using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ScottPlot;

namespace ACQ.Plot
{
    public partial class MainForm : Form
    {
        private DataTable m_dt;
        private DraggablePlot1D m_scatter;
        
        public MainForm()
        {
            InitializeComponent();

            formsPlot1.Plot.Style(Style.Seaborn);

            InitPlot(null);
        }

        void InitPlot(DataTable dt)
        {
            double[] xs, ys;

            if (dt == null)
            {
                xs = new double[] { 0, 1, 2, 3, 4 };
                ys = new double[] { 0, 1, 2, 1, 0 };
            }
            else
            {

                int n = dt.Rows.Count;
                xs = new double[n];
                ys = new double[n];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    xs[i] = (double)dt.Rows[i]["X"];
                    ys[i] = (double)dt.Rows[i]["Y"];
                    //Console.WriteLine("{0} \t {1}", xs[i], ys[i]);
                }

                Array.Sort(xs, ys);
            }

            m_scatter = new DraggablePlot1D();
            m_scatter.AddRange(xs, ys);
            m_scatter.LineWidth = 2f;
            m_scatter.MarkerSize = 5f;

            m_scatter.MovePointFunc = MoveBetweenAdjacent;
            m_scatter.Dragged += Scatter_Dragged;

            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Add(m_scatter);                        
            formsPlot1.Refresh();

            this.dataGridView1.DataSource = m_scatter.Data;
            foreach(DataGridViewColumn col in this.dataGridView1.Columns) 
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void Scatter_Dragged(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = m_scatter.Data;
        }

        // use a custom function to limit the movement of points
        static Coordinate MoveBetweenAdjacent(List<double> xs, List<double> ys, int index, Coordinate requested)
        {            
            double min_x = index <= 0             ? Double.NegativeInfinity : xs[index - 1];
            double max_x = index >= xs.Count - 1  ? Double.PositiveInfinity : xs[index + 1];            
                        
            bool isFixedY = ModifierKeys.HasFlag(Keys.Shift);

            double newX = requested.X;
            double newY = requested.Y;

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                newX = xs[index];
            }
            else
            {              
                newX = Math.Max(newX, min_x);
                newX = Math.Min(newX, max_x);
            }

            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                newY = ys[index];
            }

            return new Coordinate(newX, newY);
        }

        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            (double mouseCoordX, double mouseCoordY) = formsPlot1.GetMouseCoordinates();
            
            this.label1.Text = String.Format("X: {0}", mouseCoordX);
            this.label2.Text = String.Format("Y: {0}", mouseCoordY);

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {            
            DataTable dt = this.dataGridView1.DataSource as DataTable;
            InitPlot(dt);
        }

        private void buttonAddNode_Click(object sender, EventArgs e)
        {
            DataTable dt = this.dataGridView1.DataSource as DataTable;                       

            dt.Rows.Add(0, 0);
            
            InitPlot(dt);
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //DataTable dt = this.dataGridView1.DataSource as DataTable;
            //InitPlot(dt);            
            Console.WriteLine("remove row {0}", e.RowCount);
        }

    }
}

