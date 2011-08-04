using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using FoxEEEControl.Handlers.Classes;
using FoxEEEControl.Handlers.Types;

namespace FoxEEEControl.Handlers
{
    class MasterHandler
    {
        private List<GenericHandler> handlers = new List<GenericHandler>();
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
            handlers.Add(new GoogleHandler());

            ThreadGroup initializerGroup = new ThreadGroup();

            foreach (GenericHandler handler in handlers)
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
            foreach (GenericHandler handlerx in handlers)
            {
                GenericHandler handler = handlerx;
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

        public void Start(HandlerItem item, string curtxt)
        {
            if (item == null)
            {
                TryStart(curtxt);
                return;
            }
            try
            {
                item.handler.Start(item.text);
            }
            catch (HandlerUseShellExecException e)
            {
                if (string.IsNullOrEmpty(e.runwhat)) TryStart(item.text);
                else TryStart(e.runwhat);
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
