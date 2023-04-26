using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;


namespace ACQ.MandelbrotExplorer
{
    /// <summary>
    /// https://www.codeproject.com/Articles/1116907/How-to-Use-Your-GPU-in-NET
    /// https://www.codeproject.com/Articles/502829/GPGPU-image-processing-basics-using-OpenCL-NET
    /// 
    /// OpenCL.NET: https://github.com/lecode-official/opencl-dotnet
    /// NOpenCL: https://github.com/tunnelvisionlabs/NOpenCL
    /// </summary>
    class TestGPU
    {
        void TestCUDA()
        {
          Context context = Context.CreateDefault();
          // Prints all accelerators.
          foreach (Device d in context)
          {
              Accelerator accelerator = d.CreateAccelerator(context);
              Console.WriteLine(accelerator);
              Console.WriteLine(GetInfoString(accelerator));
          }

          // Prints all CPU accelerators.
          foreach (CPUDevice d in context.GetCPUDevices())
          {
              CPUAccelerator accelerator = (CPUAccelerator)d.CreateAccelerator(context);
              Console.WriteLine(accelerator);
              Console.WriteLine(GetInfoString(accelerator));
          }

          // Prints all Cuda accelerators.
          foreach (Device d in context.GetCudaDevices())
          {
              Accelerator accelerator = d.CreateAccelerator(context);
              Console.WriteLine(accelerator);
              Console.WriteLine(GetInfoString(accelerator));
          }

          // Prints all OpenCL accelerators.
          foreach (Device d in context.GetCLDevices())
          {
              Accelerator accelerator = d.CreateAccelerator(context);
              Console.WriteLine(accelerator);
              Console.WriteLine(GetInfoString(accelerator));
          }

          Console.WriteLine("GetPreferredDevice");
          {
              context = Context.Create(builder => builder.AllAccelerators());
              Console.WriteLine("Context: " + context.ToString());

              Device d = context.GetPreferredDevice(preferCPU: false);
              Accelerator a = d.CreateAccelerator(context);

              a.PrintInformation();
              a.Dispose();

              foreach (Device device in context.GetPreferredDevices(preferCPU: false, matchingDevicesOnly: false))
              {
                  Accelerator accelerator = device.CreateAccelerator(context);
                  accelerator.PrintInformation();
                  accelerator.Dispose();
              }
          }


          {
              HRTimer timer = new HRTimer();
              foreach (var device in context)
              {
                  timer.tic();

                  // Create accelerator for the given device
                  var accelerator = device.CreateAccelerator(context);
                  Console.WriteLine($"Performing operations on {accelerator}");

                  // Compiles and loads the implicitly grouped kernel with an automatically determined
                  // group size and an associated default stream.
                  // This function automatically compiles the kernel (or loads the kernel from cache)
                  // and returns a specialized high-performance kernel launcher.
                  // Use LoadAutoGroupedKernel to create a launcher that requires an additional accelerator-stream
                  // parameter. In this case the corresponding call will look like this:
                  // var kernel = accelerator.LoadautoGroupedKernel<Index, ArrayView<int>, int>(MyKernel);
                  // For more detail refer to the ImplicitlyGroupedKernels or ExplicitlyGroupedKer, nels sample.
                  var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, int, int>(MandelbrotKernel);

                  int x_size = 2000;
                  int y_size = 1000;

                  var buffer = accelerator.Allocate1D<int>(x_size * y_size);

                  // Launch buffer.Length many threads and pass a view to buffer
                  // Note that the kernel launch does not involve any boxing
                  kernel((int)buffer.Length, buffer.View, 2000, x_size);

                  // Reads data from the GPU buffer into a new CPU array.
                  // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
                  // that the kernel and memory copy are completed first.
                  var data = buffer.GetAsArray1D();
                  for (int i = 0, e = data.Length; i < e; ++i)
                  {

                  }
                  Console.WriteLine("Elapsed {0:F6}", timer.toc());
              }
          }
      }

      static void MandelbrotKernel(
          Index1D index,             // The global thread index (1D in this case)
          ArrayView<int> dataView,   // A view to a chunk of memory (1D in this case)
          int max_it, int width)              // A sample uniform constant
      {            

          int ix = index % width;
          int iy = index / width;

          double m_max_x_default = 0.9;
          double m_min_x_default = -2.5;
          double m_max_y_default = 0.92;
          double dx = (m_max_x_default - m_min_x_default) / (width - 1);

          double cx = m_max_x_default + ix * dx;
          double cy = m_max_y_default - iy * dx;
          // Z = X+I*Y
          double x = 0;
          double y = 0;
          int it;
          for (it = 0; it < max_it; it++)
          {
              double x2 = x * x;
              double y2 = y * y;
              // Stop iterations when |Z| > 2
              if (x2 + y2 > 4.0) break;
              double twoxy = 2.0 * x * y;
              // Z = Z^2 + C
              x = x2 - y2 + cx;
              y = twoxy + cy;
          }
          // Store the color in the A array
          dataView[index] = it;

      }

      private static string GetInfoString(Accelerator a)
      {
          System.IO.StringWriter infoString = new System.IO.StringWriter();
          a.PrintInformation(infoString);
          return infoString.ToString();
      }
    }
}
