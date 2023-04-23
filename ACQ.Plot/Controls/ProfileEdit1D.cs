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

namespace ACQ.Plot.Controls
{
    public partial class ProfileEdit1D : UserControl
    {        
        private DraggablePlot1D m_scatter;
        public ProfileEdit1D()
        {
            InitializeComponent();

            formsPlot1.Plot.Style(Style.Seaborn);

            foreach (DataGridViewColumn col in this.dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = this.dataGridView1.DataSource as DataTable;

            dt.Rows.Add(0, 0);

            InitPlot(dt);
        }

        void InitPlot(DataTable dt)
        {
            m_scatter = new DraggablePlot1D();

            if (dt != null)
            {
                double[] xs, ys;

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
                m_scatter.AddRange(xs, ys);
            }           
            
            m_scatter.LineWidth = 2f;
            m_scatter.MarkerSize = 5f;

            m_scatter.MovePointFunc = MoveBetweenAdjacent;
            m_scatter.Dragged += Scatter_Dragged;

            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Add(m_scatter);
            formsPlot1.Refresh();                        
        }

        private void Scatter_Dragged(object sender, EventArgs e)
        {
            //TODO: do this in a smart way
            this.dataGridView1.DataSource = m_scatter.Data;
            /*
            for (int i = 0; i < m_scatter.Data.Rows.Count; i++)
            {
                (this.dataGridView1.DataSource as DataTable).Rows[i]["X"] = (double)m_scatter.Data.Rows[i]["X"];
                (this.dataGridView1.DataSource as DataTable).Rows[i]["Y"] = (double)m_scatter.Data.Rows[i]["Y"];                
            }*/
        }

        // use a custom function to limit the movement of points
        static Coordinate MoveBetweenAdjacent(List<double> xs, List<double> ys, int index, Coordinate requested)
        {
            double min_x = index <= 0 ? Double.NegativeInfinity : xs[index - 1];
            double max_x = index >= xs.Count - 1 ? Double.PositiveInfinity : xs[index + 1];

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
            
            //this.label1.Text = String.Format("X: {0}", mouseCoordX);
            //this.label2.Text = String.Format("Y: {0}", mouseCoordY);
            
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataTable dt = this.dataGridView1.DataSource as DataTable;
            InitPlot(dt);
            Console.WriteLine("value changed");
        }

        public DataTable Data
        {
            set 
            {
                this.dataGridView1.DataSource = value;                

                InitPlot(value);
            }
            get 
            {
                return this.dataGridView1.DataSource as DataTable;
            }
        }

        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            DataTable dt = this.dataGridView1.DataSource as DataTable;
            InitPlot(dt);
        }
    }
}
