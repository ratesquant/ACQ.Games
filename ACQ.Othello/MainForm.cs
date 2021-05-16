using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using ACQ.Core;

namespace ACQ.Othello
{
    public partial class MainForm : Form
    {
        const string m_strUserDataFilename = "ACQ.Othello";
        ApplicationData m_data;

        const int m_nCellSize = 38;
        const int m_nMargin = 13;

        AppSettings m_settings;
        GameStatistics m_statistics;    
        Game m_game;        

        Dictionary<enPiece, Bitmap> m_vPieceImage;

        Brush m_hint_brush;
        Brush m_last_move_brush;
        Brush m_marker_brush;


        public MainForm()
        {
            InitializeComponent();

#if DEBUG
            this.aIGameToolStripMenuItem.Enabled = true;
#else
            this.aIGameToolStripMenuItem.Enabled = false;

#endif

            //Read program settings
            try
            {
                m_data = new ApplicationData(m_strUserDataFilename, true);
            }
            catch            
            {
                m_data = new ApplicationData(m_strUserDataFilename, false);
            }

            //loading of settings
            if (m_data.Count == 0)
            {
                m_settings = new AppSettings();
                m_statistics = new GameStatistics();
                m_data["Settings"]= m_settings;
                m_data["Statistics"] = m_statistics;
            }
            else
            {
                m_settings = (AppSettings)m_data["Settings"];
                m_statistics = (GameStatistics)m_data["Statistics"];
            }

            //Create game
            m_game = new Game(m_settings.HumanPlayer, m_settings.Depth, m_settings.PerfectEnding);

            //set drawing styles
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.pictureBox1.BackColor = m_settings.BoardColor;

            //load resources
            m_vPieceImage = new Dictionary<enPiece, Bitmap>();
            m_vPieceImage.Add(enPiece.Black, Othello.Properties.Resources.black_fig);
            m_vPieceImage.Add(enPiece.White, Othello.Properties.Resources.white_fig);

            //make brushes
            m_hint_brush = new SolidBrush(m_settings.HintColor);
            m_last_move_brush = new SolidBrush(m_settings.LastMoveColor);
            m_marker_brush = new SolidBrush(Color.DarkGray);

            timer1.Tick += new EventHandler(TimerEventProcessor);
            timer1.Interval = 1;
            timer1.Start();

            ResizeForm();
            UpdatedStatus();
        }

        private void UpdateScreen()
        {
            UpdatedStatus();
            this.pictureBox1.Refresh();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (m_game.Play())
            {
                UpdateScreen();
            }
        }

        private void UpdatedStatus()
        {
            if (m_game.Over)
            {
                enPiece winner = m_game.Winner;

                if (winner != enPiece.Empty)
                    this.toolStripStatusLabel1.Text = winner.ToString() + " won!";                
                else
                    this.toolStripStatusLabel1.Text = "the game is a draw!";

                //Save finished game
                if (!m_statistics.Games.ContainsValue(m_game) && !m_game.Saved)
                {   
                    m_statistics.Games.Add(DateTime.Now, m_game);
                    m_game.Saved = true;

                    //this.backgroundWorker1.RunWorkerAsync();                    
                }
            }
            else
            {
                this.toolStripStatusLabel1.Text = "Next Move: " + m_game.CurrentPlayer.ToString();
            }

            string score = String.Empty;

            score += " White: " + m_game.CurrentBoard.CountPieces(enPiece.White) + " ";
            score += " Black: " + m_game.CurrentBoard.CountPieces(enPiece.Black) + " ";

            this.toolStripStatusLabel1.Text +=  "  (" + score + ")";
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle rect = pictureBox1.DisplayRectangle;
            rect.Height--;
            rect.Width--;
            g.DrawRectangle(Pens.Black, rect);

            rect.Inflate(-m_nMargin, -m_nMargin);

            int nSize = m_game.CurrentBoard.Size;

            //show available moves
            if (m_settings.ShowAvailableMoves)
            {
                List<Move> vMoves = m_game.CurrentBoard.GetAvailableMoves(m_game.CurrentPlayer);

                foreach (Move move in vMoves)
                {
                    g.FillRectangle(m_hint_brush, new Rectangle(rect.Left + move.To.X * m_nCellSize, rect.Top + move.To.Y * m_nCellSize, m_nCellSize, m_nCellSize));
                }
            }

            //show pieces
            for (int i = 0; i < nSize; i++)
            {
                for (int j = 0; j < nSize; j++)
                {
                    enPiece piece = m_game.CurrentBoard[i, j];

                    if (m_vPieceImage.ContainsKey(piece))
                    {
                        Bitmap image = m_vPieceImage[piece];
                        g.DrawImage(image, rect.Left + i * m_nCellSize, rect.Top + j * m_nCellSize, image.Width, image.Height);
                    }
                }
            }


            //show last move
            if (m_settings.ShowLastMove)
            {
                Move move = m_game.LastMove;

                if (move != null)
                {
                    Rectangle last_move_rect = new Rectangle(rect.Left + move.To.X * m_nCellSize + 2, rect.Top + move.To.Y * m_nCellSize + 2, 5, 5);
                    g.FillEllipse(m_last_move_brush, last_move_rect);
                    g.DrawEllipse(Pens.Black, last_move_rect);
                }
            }

            //Draw Grid
            for (int i = 0; i <= nSize; i++)
            {
                g.DrawLine(Pens.Black, rect.Left + i * m_nCellSize, rect.Bottom, rect.Left + i * m_nCellSize, rect.Top);
                g.DrawLine(Pens.Black, rect.Left, rect.Top + i * m_nCellSize, rect.Right, rect.Top + i * m_nCellSize);
            }

            //Draw markers 
            int[] vMarkers = new int[] { 2, 6 };

            for (int i = 0; i < vMarkers.Length; i++)
                for (int j = 0; j < vMarkers.Length; j++)
                {     
                    Rectangle marker_rect = new Rectangle(rect.Left + vMarkers[i] * m_nCellSize - 2, rect.Top + vMarkers[j] * m_nCellSize - 2, 4, 4);
                    g.FillEllipse(m_marker_brush, marker_rect);
                    g.DrawEllipse(Pens.Black, marker_rect);
                }
        }

        public void ResizeForm()
        {
            Size size1 = this.Size;
            Size size2 = pictureBox1.Size;

            pictureBox1.Size = new Size(m_game.CurrentBoard.Size * m_nCellSize + 2 * m_nMargin + 1, m_game.CurrentBoard.Size * m_nCellSize + 2 * m_nMargin + 1);
            this.Size = pictureBox1.Size + (size1 - size2);
            pictureBox1.Refresh();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_game = new Game(m_settings.HumanPlayer, m_settings.Depth, m_settings.PerfectEnding);
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.X < m_nMargin || e.Y < m_nMargin)
                return;
            
            int pos_x = (e.X - m_nMargin) / m_nCellSize;
            int pos_y = (e.Y - m_nMargin) / m_nCellSize;

            bool bMoveMade = m_game.PlayerMove(pos_x, pos_y);

            if (bMoveMade)
            {
                UpdatedStatus();
                pictureBox1.Refresh();
            }
            Console.WriteLine("Mouse click: {0} {1}", pos_x, pos_y);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_game.UndoMove();

            UpdateScreen();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OptionsDlg dlg = new OptionsDlg(m_settings, m_statistics);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                dlg.Settings.CopyTo(m_settings);
                UpdateScreen();
            }
            else
            {
                //do nothing
            }
            dlg.Dispose();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_hint_brush.Dispose();
            m_last_move_brush.Dispose();
            m_marker_brush.Dispose();
            m_data.Save();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            m_game.Analyse();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                UpdateScreen();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Tournament tour = new Tournament();
                tour.PlayInThreads(); 
            }         
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/ratesquant/ACQ.Games");
        }

        string ListToString<T>(T[] list, string sep)
        {
            StringBuilder sb = new StringBuilder();

            foreach (T item in list)
            {
                sb.Append(item.ToString()).Append(sep);
            }
            return sb.ToString();
        }

        private void aIGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = new StreamWriter("ai.game.csv"))
            {
                sw.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}","time", "count", "mobility_weight", "mask_weight", "count_weight"));

                System.Random rng = new Random();

                for (int i = 0; i < 1; i++)
                {
                    Dictionary<enPiece, ComputerPlayer> vComputerPlayers = new Dictionary<enPiece, ComputerPlayer>();

                    int mobility_weight = 41;
                    int mask_weight = 4;
                    int count_weight = 55;
                    int total_weigth = mobility_weight + mask_weight + count_weight;
                 
                    vComputerPlayers[enPiece.White] = new ComputerPlayerBalanced(enPiece.White, 5, 20, 1, 1, true);
                    vComputerPlayers[enPiece.Black] = new ComputerPlayerMobility(enPiece.Black, 5);

                    m_game = new Game(vComputerPlayers);

                    int maxMoves = 60;
                    int[] pos_delta = new int[maxMoves];
                    int[] mov_delta = new int[maxMoves];
                    int[] mask_score = new int[maxMoves];

                    int index = 0;
                    while (!m_game.Over)
                    {
                        m_game.Play();

                        if (index < maxMoves)
                        {
                            pos_delta[index] = (m_game.CurrentBoard.CountPieces(enPiece.White) - m_game.CurrentBoard.CountPieces(enPiece.Black));
                            mov_delta[index] = (m_game.CurrentBoard.CountMoves(enPiece.White) - m_game.CurrentBoard.CountMoves(enPiece.Black));
                            //mask_score[index] = (vComputerPlayers[enPiece.Black].MaskBasedScore(m_game.CurrentBoard));
                            index++;
                        }
                    }

                    enPiece winner = m_game.Winner;

                    int white_count = m_game.CurrentBoard.CountPieces(enPiece.White);
                    int black_count = m_game.CurrentBoard.CountPieces(enPiece.Black);

                    StringBuilder sb = new StringBuilder();

                    sb.Append(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t", DateTime.Now, white_count - black_count, (double)mobility_weight / total_weigth, (double)mask_weight / total_weigth, (double)count_weight / total_weigth));
                    sb.Append("pos\t").Append(ListToString(pos_delta, "\t"));
                    sb.Append("mov\t").Append(ListToString(mov_delta, "\t"));
                    sb.Append("msk\t").Append(ListToString(mask_score, "\t"));

                    sw.WriteLine(sb.ToString());
                    sw.Flush();
                }
            }
        }
    }
}