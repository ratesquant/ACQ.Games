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
        DataTable m_dt; 
        public MainForm()
        {
            InitializeComponent();

            this.toolStripComboBox1.Items.Add("1D Profile");
            this.toolStripComboBox1.Items.Add("2D Profile");
            this.toolStripComboBox1.Items.Add("2D S-Curve");
            this.toolStripComboBox1.SelectedIndex = 0;


            //Create Example Data
            m_dt = new DataTable();
            var colx = m_dt.Columns.Add("X", typeof(double));
            var coly = m_dt.Columns.Add("Y", typeof(double));

            var xs = new double[] { 0, 1, 2, 3, 4 };
            var ys = new double[] { 0, 1, 2, 1, 0 };

            for (int i = 0; i < xs.Length; i++)
            {
                m_dt.Rows.Add(xs[i], ys[i]);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DataTable dt = this.profileEdit1D1.Data;
            this.profileEdit1D1.Data = m_dt;
        }
    }
}

