using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ACQ.Core
{
    public interface ITask
    {
        void run();
    }
    public class BlockinQueue : IDisposable
    {
        readonly object m_locker = new object();
        Thread[] m_workers;
        Queue<ITask> m_itemQ = new Queue<ITask>();

        public BlockinQueue(int workerCount)
        {
            m_workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
            {
                m_workers[i] = new Thread(Consume);
                m_workers[i].Start();
            }
        }

        public void Shutdown(bool waitForWorkers)
        {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread worker in m_workers)
                EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
            {
                foreach (Thread worker in m_workers)
                    worker.Join();
            }
        }

         public void Dispose()
         {
             Shutdown(false);
         }

        public void EnqueueItem(ITask item)
        {
            lock (m_locker)
            {
                m_itemQ.Enqueue(item);           // We must pulse because we're
                Monitor.Pulse(m_locker);         // changing a blocking condition.
            }
        }

        void Consume()
        {
            while (true)                        // Keep consuming until
            {                                   // told otherwise.
                ITask item;
                lock (m_locker)
                {
                    while (m_itemQ.Count == 0) Monitor.Wait(m_locker);
                    item = m_itemQ.Dequeue();
                }
                if (item == null) return;         // This signals our exit.
                item.run();                           // Execute item.
            }
        }
    }
}
