using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Data.SQLite;
using System.Security;

namespace FoxEEEControl
{
    public partial class frmMain : Form
    {
        private Point fixedPoint;
        private Dictionary<string, string> availableStuff = new Dictionary<string, string>();
        private HotKey hk;

        private delegate int AddItemDelegate(object obj);
        private AddItemDelegate lbResultAddItem;

        private Thread RefreshThread;
        private Thread SearchThread;

        private readonly char[] pathSep = { '/', '\\' };

        private bool textChangedAuto = false;

        private SQLiteConnection sqlite_conn;
        private SQLiteCommand sqlite_cmd;
        private SQLiteDataReader sqlite_reader;
        private object SQLiteLock = new object();

        public frmMain()
        {
            InitializeComponent();
            lbResultAddItem = new AddItemDelegate(lbResults.Items.Add);
            hk = new HotKey();
            hk.OwnerForm = this;
            hk.HotKeyPressed += new HotKey.HotKeyPressedEventHandler(hk_HotKeyPressed);
            hk.AddHotKey(Keys.Space, HotKey.MODKEY.MOD_CONTROL, "FoxEEEControl_ShowForm");
            oldCapsLock = (((ushort)GetKeyState(0x14 /*VK_CAPITAL*/)) & 0xffff) != 0;
            //oldNumLock = (((ushort)GetKeyState(0x90 /*VK_NUMLOCK*/)) & 0xffff) != 0;
            //Application.Idle += new EventHandler(Application_Idle);
            lock (SQLiteLock)
            {
                sqlite_conn = new SQLiteConnection("data source=\"Program.db\"");
                sqlite_cmd = new SQLiteCommand(sqlite_conn);
                sqlite_conn.Open();

                sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS programs(name TEXT PRIMARY KEY, path TEXT, count INT)";
                sqlite_cmd.ExecuteNonQuery();
            }
        }

        ~frmMain()
        {
            sqlite_conn.Close();
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        bool oldCapsLock;
        //bool oldNumLock;
        private void Application_Idle(object sender, EventArgs e)
        {
            bool CapsLock = (((ushort)GetKeyState(0x14 /*VK_CAPITAL*/)) & 0xffff) != 0;
            //bool NumLock = (((ushort)GetKeyState(0x90 /*VK_NUMLOCK*/)) & 0xffff) != 0;
            if (CapsLock != oldCapsLock)
            {
                notifyIcon.BalloonTipText = "CapsLock is now " + ((CapsLock) ? "ON" : "OFF");
                notifyIcon.ShowBalloonTip(1000);
                oldCapsLock = CapsLock;
            }
            /*if (NumLock != oldNumLock)
            {
                notifyIcon.BalloonTipText = "NumLock is now " + ((NumLock) ? "on" : "off");
                notifyIcon.ShowBalloonTip(1000);
                oldNumLock = NumLock;
            }*/
        }

        private void hk_HotKeyPressed(string HotKeyID)
        {
            if (HotKeyID == "FoxEEEControl_ShowForm")
            {
                this.DoShow();
            }
        }

        private void DoShow()
        {
            this.Show();
            this.Activate();
            if (tbEntry.Enabled)
            {
                tbEntry.Text = "";
                tbEntry.Focus();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Rectangle screenArea = Screen.PrimaryScreen.WorkingArea;
            this.Height = screenArea.Height;
            fixedPoint = new Point(screenArea.X + screenArea.Width, 0);
            frmMain_Move(sender, e);
            this.MinimumSize = new Size(0, screenArea.Height);
            this.MaximumSize = new Size(screenArea.Width, screenArea.Height);
            PopulateMenu(false);
        }

        private void tbEntry_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = true;
                switch (e.KeyCode)
                {
                    case Keys.Insert:
                        PopulateMenu(true);
                        break;
                    case Keys.Escape:
                        this.Hide();
                        break;
                    case Keys.Enter:
                        if (SearchThread != null && SearchThread.IsAlive) break;
                        lock (SQLiteLock)
                        {
                            this.Hide();
                            if (lbResults.Items.Count < 1 || lbResults.SelectedIndex < 0)
                            {
                                TryStart(tbEntry.Text);
                                break;
                            }
                            string item = (string)lbResults.SelectedItem;
                            if (!availableStuff.ContainsKey(item)) { TryStart(item); break; }

                            sqlite_cmd.CommandText = "UPDATE programs SET count = count + 1 WHERE name = \"" + SecurityElement.Escape(item) + "\"";
                            sqlite_cmd.ExecuteNonQuery();

                            Process p = new Process();
                            p.StartInfo = new ProcessStartInfo(availableStuff[item], "");
                            p.Start();
                        }
                        break;
                    case Keys.Down:
                        if (lbResults.Items.Count < 1) break;
                        if (lbResults.SelectedIndex >= lbResults.Items.Count - 1) lbResults.SelectedIndex = 0;
                        else lbResults.SelectedIndex++;
                        tbEntry_SetText();
                        break;
                    case Keys.Up:
                        if (lbResults.Items.Count < 1) break;
                        if (lbResults.SelectedIndex < 1) lbResults.SelectedIndex = lbResults.Items.Count - 1;
                        else lbResults.SelectedIndex--;
                        tbEntry_SetText();
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
            catch { }
        }
        private void TryStart(string stuff)
        {
            try
            {
                string txt = tbEntry.Text.Trim();
                string arg = "";
                if (!lbResults.Items.Contains(txt))
                {
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
                }
                Process px = new Process();
                px.StartInfo = new ProcessStartInfo(txt, arg);
                px.Start();
            }
            catch { this.DoShow(); }
        }
        private void tbEntry_SetText()
        {
            textChangedAuto = true;
            tbEntry.Text = (string)lbResults.SelectedItem;
            textChangedAuto = false;
            tbEntry.Select(tbEntry.Text.Length, 0);
        }
        private void tbEntry_TextChanged(object sender, EventArgs e)
        {
            if (textChangedAuto) return;
            try
            {
                SearchThread.Abort();
            }
            catch { }
            SearchThread = new Thread(new ThreadStart(SearchStuff));
            SearchThread.Start();
        }

        private void SearchStuff()
        {
            lock (SQLiteLock)
            {
                string item = "";
                string txt = tbEntry.Text;
                lbResults.Invoke(new MethodInvoker(delegate()
                {
                    item = (string)lbResults.SelectedItem;
                }));
                int selItem = 0;
                lbResults.Invoke(new MethodInvoker(lbResults.Items.Clear));
                if (txt == "") return;
                try
                {
                    NCalc.Expression exp = new NCalc.Expression(tbEntry.Text);
                    lbResults.Invoke(lbResultAddItem, exp.Evaluate() + " = " + exp.ParsedExpression.ToString());
                }
                catch { }
                if (txt.Length >= 3 && txt[1] == ':' && (txt[2] == '\\' || txt[2] == '/'))
                {
                    try
                    {
                        char c = txt[txt.Length - 1];
                        string patt = "*";
                        if (c != '/' && c != '\\')
                        {
                            int x = txt.LastIndexOfAny(pathSep) + 1;
                            patt = txt.Substring(x) + "*";
                            txt = txt.Remove(x);
                        }
                        else
                        {
                            lbResults.Invoke(lbResultAddItem, txt);
                        }
                        foreach (string s in Directory.GetFileSystemEntries(txt, patt))
                        {
                            lbResults.Invoke(lbResultAddItem, s);
                        }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        sqlite_reader.Close();
                        sqlite_reader.Dispose();
                    }
                    catch (Exception) { }

                    availableStuff.Clear();
                    sqlite_cmd.CommandText = "SELECT name, path, count FROM programs WHERE name LIKE \"%" + SecurityElement.Escape(tbEntry.Text) + "%\" ORDER BY count DESC, name ASC";
                    sqlite_reader = sqlite_cmd.ExecuteReader();
                    int namec = sqlite_reader.GetOrdinal("name");
                    int pathc = sqlite_reader.GetOrdinal("path");
                    int countc = sqlite_reader.GetOrdinal("count");
                    string tmp; string tmp2;
                    SQLiteCommand sqlc = new SQLiteCommand(sqlite_conn);
                    while (sqlite_reader.Read())
                    {
                        tmp = sqlite_reader.GetString(pathc);
                        tmp2 = sqlite_reader.GetString(namec);
                        if (!File.Exists(tmp))
                        {
                            sqlc.CommandText = "DELETE FROM programs WHERE name = \"" + SecurityElement.Escape(tmp2) + "\"";
                            sqlc.ExecuteNonQuery();
                            continue;
                        }
                        lbResults.Invoke(lbResultAddItem, tmp2);
                        availableStuff.Add(tmp2, tmp);
                    }
                    sqlite_reader.Close();
                    sqlite_reader.Dispose();
                    sqlc.Dispose();
                }
                if (lbResults.Items.Count > 0)
                {
                    lbResults.Invoke(new MethodInvoker(delegate()
                    {
                        lbResults.SelectedIndex = selItem;
                    }));
                }
            }
        }

        private void SearchRec(List<string> list, string path, string pattern)
        {
            try
            {
                foreach (string xtmp in Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly))
                {
                    list.Add(xtmp);
                }
                foreach (string xtmp in Directory.EnumerateDirectories(path))
                {
                    SearchRec(list, xtmp, pattern);
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private void PopulateMenu(bool forceRescan)
        {
            if (RefreshThread != null && RefreshThread.IsAlive) return;

            tbEntry.Enabled = false;
            lbResults.Enabled = false;
            tbEntry.Text = "Refreshing...";
            lbResults.Items.Clear();

            RefreshThread = new Thread(new ParameterizedThreadStart(__PopulateMenu));
            RefreshThread.Start(forceRescan);
        }

        private void __EndPopulateMenu()
        {
            tbEntry.Text = "";
            tbEntry.Enabled = true;
            lbResults.Enabled = true;
            tbEntry.Focus();
        }

        private void __PopulateMenu(object forceRescanO)
        {
            bool forceRescan = (bool)forceRescanO;
            lock (SQLiteLock)
            {
                if (forceRescan)
                {
                    List<string> files = new List<string>();
                    SearchRec(files, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "*.lnk");
                    SearchRec(files, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.lnk");

                    string dispName;
                    foreach (string file in files)
                    {
                        try
                        {
                            dispName = file.Remove(file.LastIndexOf('.'));
                            dispName = dispName.Substring(dispName.LastIndexOfAny(pathSep) + 1);
                            if (availableStuff.ContainsKey(dispName)) continue;
                            sqlite_cmd.CommandText = "INSERT INTO programs (name, path, count) VALUES (\"" + SecurityElement.Escape(dispName) + "\",\"" + SecurityElement.Escape(file) + "\" ,0)";
                            sqlite_cmd.ExecuteNonQuery();
                        }
                        catch { }
                    }
                }
                try
                {
                    this.Invoke(new MethodInvoker(__EndPopulateMenu));
                }
                catch { }
            }
        }

        private void frmMain_Move(object sender, EventArgs e)
        {
            this.Location = new Point(fixedPoint.X - this.Width, fixedPoint.Y);
        }

        private void tbEntry_Leave(object sender, EventArgs e)
        {
            tbEntry.Focus();
        }

        private void frmMain_Deactivate(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DoShow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopulateMenu(true);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.DoShow();
        }

        private void lbResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*textChangedAuto = true;
            string txt = tbEntry.Text;
            string it = (string)lbResults.SelectedItem;

            if (it.ToLower().IndexOf(txt.ToLower()) == 0)
            {
                tbEntry.Text = it;
                tbEntry.Select(txt.Length, tbEntry.Text.Length - txt.Length);
            }
            textChangedAuto = false;*/
        }
    }
}
