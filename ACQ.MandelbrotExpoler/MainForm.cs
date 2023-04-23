﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace ACQ.MandelbrotExplorer
{
    /// <summary>
    /// https://mandel.gart.nz/
    /// </summary>
    public partial class MainForm : Form
    {
        ColorPalette m_palette;
        DirectBitmap m_bitmap;
        int m_max_it = 200;
        Mandelbrot m_fgen;

        //mouse zoom
        bool m_zoom = false;
        Point m_zoomPoint;

        bool m_move = false;
        Point m_movePoint;

        Point m_mouse_pointer;
        
        private Dictionary<string, Type> m_available_palettes = new Dictionary<string, Type>();

        public MainForm()
        {
            InitializeComponent();
            EnumerateAvailablePalettes();

            this.pictureBox1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseWheel);

            Rectangle rect = pictureBox1.DisplayRectangle;
            m_fgen = new Mandelbrot(m_max_it, rect.Width, rect.Height);

            //set palette
            foreach (string palette_name in m_available_palettes.Keys)
            {
                this.toolStripComboBox1.Items.Add(palette_name);
            }
            this.toolStripComboBox1.SelectedItem = "Grayscale";

            foreach (int max_it in new int[] { 10, 100, m_max_it, 1000, 2000 })
            {
                this.toolStripComboBox2.Items.Add(max_it);
            }
            this.toolStripComboBox2.SelectedIndex = 2;

            UpdateMandelbrot();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.pictureBox1.Refresh();
        }        

        void EnumerateAvailablePalettes()
        {
            Type base_type = typeof(ColorPalette);

            string palette_prefix = base_type.FullName;

            Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.FullName.Contains(palette_prefix) && !t.IsAbstract && base_type.IsAssignableFrom(t) && String.Equals(t.Namespace, base_type.Namespace)).ToArray();
                        
            foreach (Type t in types)
            {
                string name = t.FullName.Replace(palette_prefix, "");                
                m_available_palettes[name] = t;
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            Rectangle rect = pictureBox1.DisplayRectangle;

            //Console.WriteLine("Mouse Wheel {0}", e.Delta);

            Point mouse_location = e.Location;

            double delta = 0.1*e.Delta / 120.0;

            int xpos_min = (int) Math.Round(mouse_location.X * delta);
            int xpos_max = (int) Math.Round(rect.Width - (rect.Width - mouse_location.X) * delta);
            int ypos_max = (int) Math.Round(mouse_location.Y * delta);

            m_fgen.SetRange(xpos_min, xpos_max, ypos_max);

            UpdateMandelbrot();

            this.pictureBox1.Refresh();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (m_bitmap != null)
            {
                g.DrawImage(m_bitmap.Bitmap, 0, 0);
            }
            
            if (m_zoom)
            {
                int xr = Math.Min(m_zoomPoint.X, m_mouse_pointer.X);
                int yr = Math.Min(m_zoomPoint.Y, m_mouse_pointer.Y);
                g.DrawRectangle(Pens.LightGray, xr, yr, Math.Abs(m_mouse_pointer.X - m_zoomPoint.X), Math.Abs(m_mouse_pointer.Y - m_zoomPoint.Y) );
            }
            
        }

        void UpdateMandelbrot()
        {
            HRTimer timer = new HRTimer();            

            m_fgen.UpdateParallel();

            double elapsed = timer.toc();

            UpdateBitmap();

            Console.WriteLine("Elapsed: {0:F2} fps ({1:F2})", 1.0/timer.toc(), 1.0/ elapsed);
        }

        void UpdateBitmap()        
        {
            if (m_bitmap == null || m_bitmap.Width != m_fgen.Width || m_bitmap.Height != m_fgen.Height)
            {
                if (m_bitmap != null)
                    m_bitmap.Dispose();
                m_bitmap = new DirectBitmap(m_fgen.Width, m_fgen.Height);
            }

            for (int i = 0; i < m_fgen.Width; i++)
            {
                for (int j = 0; j < m_fgen.Height; j++)
                {
                    m_bitmap.SetPixel(i, j, m_palette[m_fgen.IterationMap[i, j], m_fgen.MaxIt]);
                }
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            Rectangle rect = pictureBox1.DisplayRectangle;

            if (rect.Width > 0 && rect.Height > 0)
            {
                m_fgen.Resize(rect.Width, rect.Height);
            }            

            UpdateMandelbrot();
            this.pictureBox1.Refresh();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            m_mouse_pointer = e.Location;

            var point = m_fgen.GetComplexPoint(e.X, e.Y);
            int it_count = m_fgen.IterationMap[e.X, e.Y];
            double zoom_level = m_fgen.ZoomLevel;

            if (m_move)
            {
                Point mouse_location = e.Location;

                m_fgen.ShiftRange(mouse_location.X - m_movePoint.X, mouse_location.Y - m_movePoint.Y);

                m_movePoint = mouse_location;

                UpdateMandelbrot();
                this.pictureBox1.Refresh();                
            }
            if (m_zoom)
            {
                this.pictureBox1.Refresh();
            }
            
            this.toolStripStatusLabel1.Text = String.Format("Re: {0:F12} Im: {1:F12} (it: {2}), Zoom: {3:F1}", point.Item1, point.Item2, it_count, zoom_level);            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                m_zoom = true;
                m_zoomPoint = e.Location;
            }

            if (e.Button == MouseButtons.Left)
            {
                m_move = true;
                m_movePoint = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_move = false;
            }

            if (e.Button == MouseButtons.Middle)
            {
                if (m_zoom)
                {
                    m_zoom = false;

                    Point mouse_location = e.Location;

                    m_fgen.SetRange(Math.Min(m_zoomPoint.X, mouse_location.X), Math.Max(m_zoomPoint.X, mouse_location.X), Math.Min(m_zoomPoint.Y, mouse_location.Y));

                    UpdateMandelbrot();

                    this.pictureBox1.Refresh();                    
                }
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_fgen.SetDefaultRange();

            UpdateMandelbrot();
            this.pictureBox1.Refresh();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            UpdateMandelbrot();
            this.pictureBox1.Refresh();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            AboutBox aboutDialog = new AboutBox();

            aboutDialog.ShowDialog();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string palette_name = this.toolStripComboBox1.SelectedItem.ToString();

            if (m_available_palettes.ContainsKey(palette_name))
            {
                m_palette  = Activator.CreateInstance(m_available_palettes[palette_name], 256) as ColorPalette;
                
                UpdateBitmap();
                this.pictureBox1.Refresh();
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_max_it = (int)this.toolStripComboBox2.SelectedItem;

            m_fgen.MaxIt = m_max_it;

            UpdateBitmap();
            this.pictureBox1.Refresh();
        }
    }

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color color)
        {
            int index = x + (y * Width);
            int col = color.ToArgb();

            Bits[index] = col;
        }

        public void SetPixel(int x, int y, int color)
        {
            int index = x + (y * Width);            

            Bits[index] = color;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}