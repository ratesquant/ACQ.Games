using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ACQ.Othello
{
    public partial class OptionsDlg : Form
    {
        AppSettings m_settings; //copy of settings
        GameStatistics m_statistics;
        
        /// <summary>
        /// Options Dlg does not modify setings and statisttics
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="statistics"></param>
        public OptionsDlg(AppSettings settings, GameStatistics statistics)
        {            
            InitializeComponent();
            
            m_settings = (AppSettings)settings.Clone();
            m_statistics = statistics;         
            
            this.comboBox2.Items.Add(enPiece.White);
            this.comboBox2.Items.Add(enPiece.Black);

            UpdateConstrols();
        }

        public AppSettings Settings
        {
            get
            {
                return m_settings;
            }
        }

        private void UpdateConstrols()
        {
            this.numericUpDownDepth.Value = m_settings.Depth;
            this.comboBox2.SelectedItem = m_settings.HumanPlayer;

            this.checkBox1.Checked = m_settings.PerfectEnding;
            this.checkBox2.Checked = m_settings.ShowAvailableMoves;
            this.checkBox3.Checked = m_settings.ShowLastMove;

            listView1.Columns.Add("Depth", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Games Won", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Total Games", -2, HorizontalAlignment.Left);


            //Statistics
            List<ListViewItem> vItems = new List<ListViewItem>();
            for (int depth = 1; depth < 7; depth++ )
            {
                ListViewItem item = new ListViewItem(depth.ToString(), 0);
                item.SubItems.Add(m_statistics.CountWins(depth).ToString());
                item.SubItems.Add(m_statistics.CountGames(depth).ToString());
                vItems.Add(item);
            }

            listView1.Items.AddRange(vItems.ToArray());
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_settings.PerfectEnding = this.checkBox1.Checked;
            m_settings.ShowAvailableMoves = this.checkBox2.Checked;
            m_settings.ShowLastMove = this.checkBox3.Checked;
            m_settings.Depth = (int)this.numericUpDownDepth.Value;
            m_settings.HumanPlayer = (enPiece)this.comboBox2.SelectedItem;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_settings.Reset();
            UpdateConstrols();
        }
    }
}