using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACQ.MandelbrotExplorer
{
    /// <summary>
    /// Iterate z[n+1] = z[n] * z[n] + c, starting with z[0] = 0
    /// 
    /// dx = (m_max_x - m_min_x) / (Width - 1)
    /// Mapping X = min_x + dx * PointX #PointX [0, Width - 1]
    /// Mapping Y = max_y - dx * PointY #PointY [0, Height - 1]
    /// 
    /// m_y_center + (0.5 * ny - ypos) * dx
    /// </summary>
    class Mandelbrot
    {
        private const double m_max_x_default = 0.9;
        private const double m_min_x_default = -2.5;
        private const double m_max_y_default = 0.92;

        private int m_max_it;
        private double m_max_x, m_min_x;        
        private double m_max_y; //we keep aspect ratio constant, so min_y will be determined by the size of the window

        int[,] m_it_map;

        public Mandelbrot(int max_it, int xsize, int ysize)
        {
            m_max_it = max_it;
            m_it_map = new int[xsize, ysize];

            SetDefaultRange();
        }

        public Mandelbrot(int max_it, int xsize, int ysize, double min_x, double max_x, double max_y)
        {
            m_max_it = max_it;
            m_it_map = new int[xsize, ysize];

            m_max_x = max_x;
            m_min_x = min_x;
            m_max_y = max_y;
        }

        public int MaxIt
        {
            get 
            {
                return m_max_it;
            }
            set 
            {
                m_max_it = value;
                UpdateParallel();
            }
        }

        public double MaxX
        {
            get
            {
                return m_max_x;
            }
        }

        public double MinX
        {
            get
            {
                return m_min_x;
            }
        }

        public double MaxY
        {
            get
            {
                return m_max_y;
            }
        }

        public int[,] IterationMap
        {
            get
            {
                return m_it_map;
            }
        }

        public int Width
        {
            get
            {
                return m_it_map.GetLength(0);
            }
        }

        public int Height
        {
            get
            {
                return m_it_map.GetLength(1);
            }
        }

        public Tuple<double, double> RangeX 
        {
            get 
            {
                return Tuple.Create(m_min_x, m_max_x);
            }
        }

        public Tuple<double, double> RangeY
        {
            get
            {
                return Tuple.Create(m_min_x, m_max_x);
            }
        }

        public Tuple<double, double> GetComplexPoint(int xpos, int ypos)
        {
            int nx = this.Width;
            int ny = this.Height;

            double alpha_x = (double) xpos / (nx - 1);
            double dx = (m_max_x - m_min_x) / (nx - 1);            

            return Tuple.Create(m_min_x * (1.0 - alpha_x) + m_max_x * alpha_x, m_max_y - (ypos) * dx);
        }

        public void Resize(int xsize, int ysize)
        {
            m_it_map = new int[xsize, ysize];
        }

        public void SetDefaultRange()
        {
            m_max_x = m_max_x_default;
            m_min_x = m_min_x_default;
            m_max_y = m_max_y_default;
        }

        public double ZoomLevel
        {
            get
            {
                return (m_max_x_default - m_min_x_default) / (m_max_x - m_min_x);
            }
        }


        public void SetRange(int xpos_min, int xpos_max, int ypos_max)
        {
            int nx = this.Width;
            int ny = this.Height;
            double dx = (m_max_x - m_min_x) / (nx - 1);

            m_max_x = m_min_x + xpos_max * dx;
            m_min_x = m_min_x + xpos_min * dx;

            m_max_y = m_max_y - ypos_max * dx;
        }

        public void ShiftRange(int xpos_delta, int ypos_delta)
        {
            int nx = this.Width;
            int ny = this.Height;
            double dx = (m_max_x - m_min_x) / (nx - 1);

            double x_delta = xpos_delta * dx;
            
            m_min_x = m_min_x - x_delta;
            m_max_x = m_max_x - x_delta;

            m_max_y = m_max_y + ypos_delta * dx;
        }

        public void Update()
        {
            int nx = this.Width;
            int ny = this.Height;

            double dx = (m_max_x - m_min_x) / (nx - 1);            

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double x0 = m_min_x + i * dx; // current real value
                    double y0 = m_max_y - j * dx; // current imaginary value
                    
                    double z_real = x0;
                    double z_imag = y0;

                    m_it_map[i, j] = m_max_it;

                    for (int k = 0; k < m_max_it; ++k)
                    {
                        double r2 = z_real * z_real;
                        double i2 = z_imag * z_imag;

                        if (r2 + i2 > 4.0) 
                        {
                            m_it_map[i, j] = k;                            
                            break;
                        }

                        z_imag = 2.0 * z_real * z_imag + y0;
                        z_real = r2 - i2 + x0;
                    }                    
                }
            }
        }

        public void UpdateParallel()
        {
            int nx = this.Width;
            int ny = this.Height;

            double dx = (m_max_x - m_min_x) / (nx - 1);            

            Parallel.For(0, nx, i =>
            {
                for (int j = 0; j < ny; j++)
                {
                    double x0 = m_min_x + i * dx; // current real value
                    double y0 = m_max_y - j * dx; // current imaginary value

                    double z_real = x0;
                    double z_imag = y0;

                    m_it_map[i, j] = m_max_it;

                    for (int k = 0; k < m_max_it; ++k)
                    {
                        double r2 = z_real * z_real;
                        double i2 = z_imag * z_imag;

                        if (r2 + i2 > 4.0)
                        {
                            m_it_map[i, j] = k;
                            break;
                        }

                        z_imag = 2.0 * z_real * z_imag + y0;
                        z_real = r2 - i2 + x0;
                    }
                }
            });
        }
    }
}