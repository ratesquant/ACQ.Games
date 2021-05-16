using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ACQ.Core
{
    public interface ICompressor
    {
        byte[] Compress(byte[] input);
        byte[] Decompress(byte[] input);
    }
    /// <summary>
    /// Uses GZipStream: GZip = Deflate + Header w/Checksum
    /// </summary>
    public class GZip : ICompressor
    {
        private static int m_defaultBufferSize = 4096;

        /// <summary>
        /// Maximum entropy is 8 (uniform), minimum is 0 
        /// Large entropy indicates good compression
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double ShannonEntropy(byte[] input)
        {
            if (input == null)
                return Double.NaN;

            uint[] acc = new uint[256];

            for (int i = 0; i < input.Length; i++)
                acc[input[i]]++;

            double entropy = 0;            
            for (int i = 0; i < acc.Length; i++)
            {
                // if acc[i] = 0, p * log(p) = 0 
                if (acc[i] != 0)
                {
                    double p = (double)acc[i] / input.Length;
                    entropy += p * Math.Log(p, 2);
                }
            }

            return -entropy;
        }

        #region Compression        

        public byte[] Compress(byte[] input)
        {
            byte[] result = null;

            using (MemoryStream stream = new MemoryStream(input.Length))
            {
                using (GZipStream compressor = new GZipStream(stream, CompressionMode.Compress))                
                {
                    compressor.Write(input, 0, input.Length);
                }
                result = stream.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Compresses bytes read from input stream and writes them to output stream.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void Compress(Stream input, Stream output)
        {            
            Compress(input, output, (int)input.Length);
        }
        public void Compress(Stream input, Stream output, int bufferSize)
        {
            using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true))
            {                
                byte[] buffer = new byte[bufferSize];
                
                int bytesRead = 0;

                while(true)
                {
                    bytesRead = input.Read(buffer, 0, bufferSize);

                    if (bytesRead > 0)
                        gzip.Write(buffer, 0, bytesRead);
                    else
                        break;
                }
            }            
        }
        #endregion 

        #region Decompression
        public byte[] Decompress(byte[] input)
        {
            byte[] result = null;

            using (GZipStream gzip = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))            
            {
                result = ReadAllBytes(gzip, m_defaultBufferSize);               
            }
            return result;
        }

        public void Decompress(Stream input, Stream output)
        {
            Decompress(input, output, m_defaultBufferSize);
        }
        public void Decompress(Stream input, Stream output, int bufferSize)
        {
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress, true))
            {
                byte[] buffer = new byte[bufferSize];

                int bytesRead = 0;

                while (true)
                {
                    bytesRead = gzip.Read(buffer, 0, bufferSize);

                    if (bytesRead > 0)
                        output.Write(buffer, 0, bytesRead);
                    else
                        break;
                }                
            }           
        }
        #endregion

        private static byte[] ReadAllBytes(Stream stream, int bufferSize)
        {
            byte[] result = null;
            byte[] buffer = new byte[bufferSize];
            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead = 0;
                while(true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                        ms.Write(buffer, 0, bytesRead);
                    else
                        break;
                    
                };

                result = ms.ToArray();
            }
            return result;
        }       


        public static void Test()
        {
            string text = @"C#(pronounced see sharp) is a multi-paradigm programming language encompassing strong typing, imperative, declarative, functional, generic, object-oriented (class-based), and component-oriented programming disciplines. It was developed by Microsoft within its .NET initiative and later approved as a standard by Ecma (ECMA-334) and ISO (ISO/IEC 23270:2006). C# is one of the programming languages designed for the Common Language Infrastructure.";

            GZip zipper = new GZip();

            byte[] compressed = zipper.Compress(Encoding.UTF8.GetBytes(text));
            byte[] decompressed = zipper.Decompress(compressed);

            string output = Encoding.UTF8.GetString(decompressed);

            Console.WriteLine("Decompression {0}: {1}", output.Equals(text), output);            
        }

        public static void Test2()
        {
            string text = @"C#(pronounced see sharp) is a multi-paradigm programming language encompassing strong typing, imperative, declarative, functional, generic, object-oriented (class-based), and component-oriented programming disciplines. It was developed by Microsoft within its .NET initiative and later approved as a standard by Ecma (ECMA-334) and ISO (ISO/IEC 23270:2006). C# is one of the programming languages designed for the Common Language Infrastructure.";

            GZip zipper = new GZip();

            //write string to a text
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(text);

            //compress
            MemoryStream out_stream = new MemoryStream();
            stream.Position = 0;
            zipper.Compress(stream, out_stream);

            MemoryStream decompressed = new MemoryStream();
            out_stream.Position = 0;
            zipper.Decompress(out_stream, decompressed);

            decompressed.Position = 0;
            BinaryReader reader = new BinaryReader(decompressed);
            string output = reader.ReadString();
            
            Console.WriteLine("Decompression {0}: {1}", output.Equals(text), output);
        }

    }  
}
