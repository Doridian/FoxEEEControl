using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using FoxEEEControl.Handlers.Classes;

namespace FoxEEEControl.Handlers
{
    class MasterHandler
    {
        private List<IHandler> handlers = new List<IHandler>();
        private Thread RefreshThread;

        public void Initialize(object param)
        {
            try
            {
                RefreshThread.Abort();
            }
            catch { }

            RefreshThread = new Thread(new ParameterizedThreadStart(__Initialize));
            RefreshThread.Start(param);
        }

        private void __Initialize(object param)
        {
            Program.mainForm.Invoke(new MethodInvoker(Program.mainForm.StartRefresh));

            handlers.Clear();

            handlers.Add(new StartMenuHandler());
            handlers.Add(new NCalcHandler());
            handlers.Add(new DirectoryBrowserHandler());

            ThreadGroup initializerGroup = new ThreadGroup();

            foreach (IHandler handler in handlers)
            {
                try
                {
                    initializerGroup.AddAndRun(handler.Initialize, param);
                }
                catch { }
            }

            initializerGroup.WaitForCompletion();

            Program.mainForm.Invoke(new MethodInvoker(Program.mainForm.EndRefresh));
        }

        public HandlerItem[] GetResultsFor(string search)
        {
            ThreadGroup searchGroup = new ThreadGroup();
            List<HandlerItem> ret = new List<HandlerItem>();
            foreach (IHandler handlerx in handlers)
            {
                IHandler handler = handlerx;
                searchGroup.AddAndRun(delegate()
                {
                    HandlerItem[]  tmp = handler.GetResultsFor(search);
                    if (tmp == null) return;
                    lock (ret)
                    {
                        ret.AddRange(tmp);
                    }
                });
            }
            searchGroup.WaitForCompletion();
            return ret.ToArray();
        }

        public void Start(HandlerItem item)
        {
            try
            {
                item.handler.Start(item.text);
            }
            catch (HandlerUseShellExecException)
            {
                TryStart(item.text);
            }
            catch
            {
                TryStart(item.text); //fallback!
            }
        }

        private void TryStart(string txt)
        {
            try
            {
                string arg = "";
                int x;
                if (txt[0] == '"')
                {
                    x = txt.IndexOf('"', 1);
                    if (x < 0) return;
                    arg = txt.Substring(x + 1).Trim();
                    txt = txt.Remove(x).Substring(1);
                }
                else if ((x = txt.IndexOf(' ')) >= 0)
                {
                    arg = txt.Substring(x + 1).Trim();
                    txt = txt.Remove(x);
                }
                Process px = new Process();
                px.StartInfo = new ProcessStartInfo(txt, arg);
                px.Start();
            }
            catch { MessageBox.Show("Unable to start!"); }
        }
    }
}
