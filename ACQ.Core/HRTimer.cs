using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


//High performance timer
namespace ACQ.Core
{
    /// <summary>
    /// Simple High Performance Timer:
    /// HRTimer timer = new HRTimer();
    /// timer.tic();
    /// -----//-----
    /// timer.toc();
    /// </summary>
    public class HRTimer
    {
        private Stopwatch m_timer;
        private long m_elapsedTicks;
        private long m_nFreq;

        private static bool m_bCreated = false; //true if there are objects of Timer class

        public HRTimer()
        {
            m_timer    = new Stopwatch();

            m_nFreq = Stopwatch.Frequency;

            System.Diagnostics.Debug.WriteLineIf(!Stopwatch.IsHighResolution, "high-resolution performance counter is not available");
            System.Diagnostics.Debug.WriteLineIf(!m_bCreated, "Timer resolution " + (1e9/m_nFreq).ToString() + " ns");

            m_bCreated = true;

            tic();
        }

        public void tic()
        {
            m_timer.Reset();
            m_timer.Start();
            m_elapsedTicks = m_timer.ElapsedTicks;
        }
        /// <summary>
        /// returns time interval in seconds from the last tic call
        /// </summary>
        /// <returns>Time in seconds, since last call of tic</returns>
        public double toc()
        {
            return (double)(m_timer.ElapsedTicks - m_elapsedTicks) / m_nFreq;
        }

        /// <summary>
        /// returns time span object
        /// </summary>
        /// <returns>time span object, time since creation of Timer</returns>
        public TimeSpan GetTimeSpan()
        {
            return m_timer.Elapsed;
        }

    }

}