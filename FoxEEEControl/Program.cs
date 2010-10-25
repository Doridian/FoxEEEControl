using System;
using System.Threading;
using System.Windows.Forms;

namespace FoxEEEControl
{
    static class Program
    {
        static Mutex mainMutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool firstInstance;
            mainMutex = new Mutex(false, @"Local\FoxEEEControl_Mutex", out firstInstance);
            if (!firstInstance) { MessageBox.Show("Another instance is already running!", "FoxEEEControl", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
