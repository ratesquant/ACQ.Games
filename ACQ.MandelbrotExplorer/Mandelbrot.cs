#define GPU

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if GPU
using ILGPU;
using ILGPU.Runtime;
#endif

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
#if GPU
        private MandelbrotGPU m_gpu_calc = null;
#endif  

        private const double m_max_x_default = 0.9;
        private const double m_min_x_default = -2.5;
        private const double m_max_y_default = 0.92;

        private int m_max_it;
        private double m_max_x, m_min_x;
        private double m_max_y; //we keep aspect ratio constant, so min_y will be determined by the size of the window

        int[,] m_it_map;
        int[,] m_it_map_temp;

        public Mandelbrot(int max_it, int xsize, int ysize)
        {
            m_max_it = max_it;
            m_it_map = new int[xsize, ysize];

            try
            {
                m_gpu_calc = new MandelbrotGPU();
            }
            catch (Exception e)
            {
                m_gpu_calc = null;
                System.Diagnostics.Debug.Write(e.Message);
            }

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

            double alpha_x = (double)xpos / (nx - 1);
            double alpha_y = (double)ypos / (ny - 1);
            double min_y = m_max_y - (ny - 1) * (m_max_x - m_min_x) / (nx - 1);            

            return Tuple.Create(m_min_x * (1.0 - alpha_x) + m_max_x * alpha_x, min_y * (1.0 - alpha_y) + m_max_y * alpha_y);
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

            double x_range = (m_max_x - m_min_x);
            double x_delta = x_range * xpos_delta / (nx - 1);

            m_min_x = m_min_x - x_delta;
            m_max_x = m_max_x - x_delta;
            m_max_y = m_max_y + x_range * ypos_delta / (nx - 1);

            if (m_it_map_temp == null || m_it_map_temp.GetLength(0) != nx  || m_it_map_temp.GetLength(1) != ny)
            {
                m_it_map_temp = new int[nx, ny];
            }

            //Console.WriteLine("shift: {0} {1}", xpos_delta, ypos_delta);

            int x_offset_pos = Math.Min(nx, Math.Max(0, xpos_delta)); //positive offset
            int y_offset_pos = Math.Min(ny, Math.Max(0, ypos_delta));

            int x_offset_neg = Math.Min(nx, -Math.Min(0, xpos_delta));//negative offset
            int y_offset_neg = Math.Min(ny, -Math.Min(0, ypos_delta));
            for (int i = x_offset_pos; i <nx - x_offset_neg; i++)
            {
                for (int j = y_offset_pos; j < ny - y_offset_neg; j++)
                {
                    m_it_map_temp[i, j] = m_it_map[i - x_offset_pos + x_offset_neg, j - y_offset_pos + y_offset_neg];
                }
            }

            //swap pointers
            int[,] temp = m_it_map_temp;
            m_it_map_temp = m_it_map;
            m_it_map = temp;            

            UpdatePartialParallel(0, x_offset_pos, 0, ny);
            UpdatePartialParallel(nx - x_offset_neg, nx, 0, ny);

            UpdatePartialParallel(x_offset_pos, nx - x_offset_neg, 0, y_offset_pos);
            UpdatePartialParallel(x_offset_pos, nx - x_offset_neg, ny - y_offset_neg, ny);
            //UpdatePartialParallel();
        }

        public void Update()
        {            
            if (m_gpu_calc != null)
            {
                m_gpu_calc.Update(this.Width, this.Height, m_max_it, m_min_x, m_max_x, m_max_y, m_it_map);                
            }
            else
            {
                UpdateParallel();                
            }
        }

        private void UpdateBasic()
        {
            int nx = this.Width;
            int ny = this.Height;
            
            double min_y = m_max_y - (ny - 1) * (m_max_x - m_min_x) / (nx - 1);

            for (int i = 0; i < nx; i++)
            {
                double alpha_x = (double)i / (nx - 1);
                double x0 = m_min_x * (1.0 - alpha_x) + m_max_x * alpha_x;

                for (int j = 0; j < ny; j++)
                {
                    double alpha_y = (double)j / (ny - 1);
                    double y0 = min_y * (1.0 - alpha_y) + m_max_y * alpha_y;
                    
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

        private void UpdateParallel()
        {
            UpdatePartialParallel(0, this.Width, 0, this.Height);
        }

        private void UpdatePartialParallel(int i_min, int i_max, int j_min, int j_max)
        {
            int nx = this.Width;
            int ny = this.Height;

            double min_y = m_max_y - (ny - 1) * (m_max_x - m_min_x) / (nx - 1);

            //for (int i = 0; i < nx; i++)
            Parallel.For(i_min, i_max, i =>
            {
                double alpha_x = (double)i / (nx - 1);
                double x0 = m_min_x * (1.0 - alpha_x) + m_max_x * alpha_x;

                for (int j = j_min; j < j_max; j++)
                {
                    double alpha_y = (double)j / (ny - 1);
                    double y0 = m_max_y * (1.0 - alpha_y) + min_y * alpha_y;// current imaginary value                    

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

    #if GPU
    class MandelbrotGPU
    {
        Context m_context;
        Accelerator m_accelerator;
        bool m_disposed;
        Action<Index1D, ArrayView<int>, int, int, int, double, double, double, double> m_kernel;
        Action<Index1D, ArrayView<int>, int, int, int, float, float, float, float> m_kernel_float;
        public MandelbrotGPU()
        {
            m_context = Context.CreateDefault();
            m_accelerator = m_context.GetPreferredDevice(preferCPU: false).CreateAccelerator(m_context);            
            m_disposed = false;
            m_kernel = m_accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, int, int, int, double, double, double, double>(MandelbrotKernel);
            m_kernel_float = m_accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, int, int, int, float, float, float, float>(MandelbrotKernel_float);

            Console.WriteLine("{0}", GetInfoString(m_accelerator));
        }

        private static string GetInfoString(Accelerator a)
        {
            System.IO.StringWriter infoString = new System.IO.StringWriter();
            a.PrintInformation(infoString);
            return infoString.ToString();
        }

        public void Update(int nx, int ny, int max_it, double min_x, double max_x, double max_y, int[,] it_map)
        {
            double min_y = max_y - (ny - 1) * (max_x - min_x) / (nx - 1);

            int total_size = nx * ny;
            var buffer = m_accelerator.Allocate1D<int>(total_size);

            // Launch buffer.Length many threads and pass a view to buffer
            // Note that the kernel launch does not involve any boxing
            //
            //m_kernel_float((int)buffer.Length, buffer.View, max_it, nx, ny, (float)min_x, (float)max_x, (float)min_y, (float)max_y);
            m_kernel((int)buffer.Length, buffer.View, max_it, nx, ny, min_x, max_x, min_y, max_y);

            // Reads data from the GPU buffer into a new CPU array.
            // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
            // that the kernel and memory copy are completed first.

            m_accelerator.Synchronize();

            var data = buffer.GetAsArray1D();

            for (int i = 0; i < total_size; i++)
            {
                it_map[i % nx, i / nx] = data[i];
            }           
        }

        static void MandelbrotKernel(
           Index1D index,             // The global thread index (1D in this case)            
           ArrayView<int> dataView,   // A view to a chunk of memory (1D in this case)
           int max_it, int nx, int ny, double min_x, double max_x, double min_y, double max_y)              // A sample uniform constant
        {   
            int iy = index / nx;

            double alpha_x = (double)(index % nx) / (nx - 1);
            double x0 = min_x * (1.0 - alpha_x) + max_x * alpha_x;

            double alpha_y = (double)(iy) / (ny - 1);
            double y0 = max_y * (1.0 - alpha_y) + min_y * alpha_y;// current imaginary value                                

            double z_real = x0;
            double z_imag = y0;

            dataView[index] = max_it;

            for (int k = 0; k < max_it; ++k)
            {
                double r2 = z_real * z_real;
                double i2 = z_imag * z_imag;

                if (r2 + i2 > 4.0)
                {
                    dataView[index] = k;
                    break;
                }

                z_imag = 2.0 * z_real * z_imag + y0;
                z_real = r2 - i2 + x0;
            }
        }

        static void MandelbrotKernel_float(
    Index1D index,             // The global thread index (1D in this case)            
    ArrayView<int> dataView,   // A view to a chunk of memory (1D in this case)
    int max_it, int nx, int ny, float min_x, float max_x, float min_y, float max_y)              // A sample uniform constant
        {
            int ix = index % nx;
            int iy = index / nx;

            float alpha_x = (float)ix / (nx - 1);
            float x0 = min_x * (1.0f - alpha_x) + max_x * alpha_x;

            float alpha_y = (float)iy / (ny - 1);
            float y0 = max_y * (1.0f - alpha_y) + min_y * alpha_y;// current imaginary value                    

            float z_real = x0;
            float z_imag = y0;

            dataView[index] = max_it;

            for (int k = 0; k < max_it; ++k)
            {
                float r2 = z_real * z_real;
                float i2 = z_imag * z_imag;

                if (r2 + i2 > 4.0f)
                {
                    dataView[index] = k;
                    break;
                }

                z_imag = 2.0f * z_real * z_imag + y0;
                z_real = r2 - i2 + x0;
            }
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_disposed = true;

            m_accelerator.Dispose();
            m_context.Dispose();
        }
    }
#endif
}