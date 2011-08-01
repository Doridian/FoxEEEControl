using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FoxEEEControl
{
    class ThreadGroup
    {
        private List<Thread> threads = new List<Thread>();

        public Thread Add(ThreadStart ts)
        {
            Thread t = new Thread(ts);
            Add(t);
            return t;
        }

        public Thread Add(ParameterizedThreadStart ts)
        {
            Thread t = new Thread(ts);
            Add(t);
            return t;
        }

        public void Add(Thread t)
        {
            threads.Add(t);
        }

        public void AddAndRun(Thread t)
        {
            Add(t);
            RunSingle(t);
        }

        public void AddAndRun(ThreadStart ts)
        {
            RunSingle(Add(ts));
        }

        public void AddAndRun(ParameterizedThreadStart ts, object param)
        {
            RunSingle(Add(ts), param);
        }

        public void AddAndRun(Thread t, object param)
        {
            Add(t);
            RunSingle(t, param);
        }

        public void Run()
        {
            foreach (Thread t in threads)
            {
                RunSingle(t);
            }
        }

        public void Run(object param)
        {
            foreach (Thread t in threads)
            {
                RunSingle(t, param);
            }
        }

        public void Abort()
        {
            foreach (Thread t in threads)
            {
                try
                {
                    t.Abort();
                }
                catch { }
            }
        }

        public bool IsRunning()
        {
            foreach (Thread t in threads)
            {
                if (t.IsAlive) return true;
            }
            return false;
        }

        public void WaitForCompletion()
        {
            while (IsRunning())
            {
                Thread.Sleep(10);
            }
        }

        private void RunSingle(Thread t)
        {
            if (!CanRun(t)) return;
            t.Start();
        }

        private void RunSingle(Thread t, object param)
        {
            if (!CanRun(t)) return;
            t.Start(param);
        }

        private bool CanRun(Thread t)
        {
            return t.ThreadState == ThreadState.Unstarted;
        }
    }
}
