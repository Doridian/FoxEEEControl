using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FoxEEEControl.Handlers;

namespace FoxEEEControl
{
    public partial class frmMain : Form
    {
        private Point fixedPoint;
        private HotKey hk;

        private delegate int AddItemDelegate(object obj);
        private AddItemDelegate lbResultAddItem;

        private Thread SearchThread;
        private MasterHandler masterHandler;

        public frmMain()
        {
            InitializeComponent();
            lbResultAddItem = new AddItemDelegate(lbResults.Items.Add);
            hk = new HotKey();
            hk.OwnerForm = this;
            hk.HotKeyPressed += new HotKey.HotKeyPressedEventHandler(hk_HotKeyPressed);
            hk.AddHotKey(Keys.Space, HotKey.MODKEY.MOD_CONTROL, "FoxEEEControl_ShowForm");
            bool firstRun = !SQLiteHandler.DBExists();
            SQLiteHandler.Open();
            masterHandler = new MasterHandler();
            masterHandler.Initialize(firstRun);
        }

        ~frmMain()
        {
            SQLiteHandler.Close();
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
        }

        private void tbEntry_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = true;
                switch (e.KeyCode)
                {
                    case Keys.Insert:
                        masterHandler.Initialize(true);
                        break;
                    case Keys.Escape:
                        this.Hide();
                        break;
                    case Keys.Enter:
                        if (SearchThread != null && SearchThread.IsAlive) break;
                        this.Hide();
                        try
                        {
                            masterHandler.Start((HandlerItem)lbResults.SelectedItem, tbEntry.Text);
                        }
                        catch
                        {
                            this.DoShow();
                        }
                        break;
                    case Keys.Down:
                        if (lbResults.Items.Count < 1) break;
                        if (lbResults.SelectedIndex >= lbResults.Items.Count - 1) lbResults.SelectedIndex = 0;
                        else lbResults.SelectedIndex++;
                        tbDisplay_SetText();
                        SaveSelectedItem();
                        break;
                    case Keys.Up:
                        if (lbResults.Items.Count < 1) break;
                        if (lbResults.SelectedIndex < 1) lbResults.SelectedIndex = lbResults.Items.Count - 1;
                        else lbResults.SelectedIndex--;
                        tbDisplay_SetText();
                        SaveSelectedItem();
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
            catch { }
        }

        private void tbEntry_TextChanged(object sender, EventArgs e)
        {
            tbDisplay_SetText();
            try
            {
                SearchThread.Abort();
            }
            catch { }
            SearchThread = new Thread(new ThreadStart(SearchStuff));
            SearchThread.Start();
        }

        private HandlerItem selItem;
        private void SaveSelectedItem()
        {
            selItem = (HandlerItem)lbResults.SelectedItem;
        }
        private void SearchStuff()
        {
            string txt = tbEntry.Text;
            lbResults.Invoke(new MethodInvoker(lbResults.Items.Clear));
            if (txt == "") return;

            HandlerItem[] items = masterHandler.GetResultsFor(txt);
            
            lbResults.Invoke(new MethodInvoker(delegate()
            {
                lbResults.Items.AddRange(items);
            }));
            if (selItem != null && lbResults.Items.Count > 0)
            {
                for (int i = 0; i < lbResults.Items.Count; i++)
                {
                    HandlerItem item = (HandlerItem)lbResults.Items[i];
                    if (item.Equals(selItem))
                    {
                        lbResults.Invoke(new MethodInvoker(delegate()
                        {
                            lbResults.SelectedIndex = i;
                            tbDisplay_SetText();
                        }));
                        break;
                    }
                }
            }
        }

        private void __tbDisplay_SetEntryText()
        {
            tbDisplay.Text = (string)tbEntry.Text.Clone();
            tbDisplay.SelectAll();
            tbDisplay.SelectionBackColor = Color.Transparent;
            tbDisplay.SelectionColor = Color.Black;
        }

        private void tbDisplay_SetText()
        {
            string str = tbEntry.Text.ToLower();

            if (lbResults.SelectedItem == null) { __tbDisplay_SetEntryText(); return; }

            HandlerItem item = (HandlerItem)lbResults.SelectedItem;
            tbDisplay.Text = item.handler.GetTextForItem(item.text);
            if (string.IsNullOrWhiteSpace(tbDisplay.Text) || tbDisplay.Text.ToLower().IndexOf(str) < 0) { __tbDisplay_SetEntryText(); return; }

            int index = tbDisplay.Text.ToLower().IndexOf(tbEntry.Text.ToLower());
            if (index < 0) { __tbDisplay_SetEntryText(); return; }

            tbDisplay.SelectAll();
            tbDisplay.SelectionBackColor = Color.Transparent;
            tbDisplay.SelectionColor = Color.Black;
            tbDisplay.Select(index, tbEntry.Text.Length);
            tbDisplay.SelectionBackColor = Color.Gray;
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
            masterHandler.Initialize(true);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.DoShow();
        }


        public void StartRefresh()
        {
            tbEntry.Enabled = false;
            lbResults.Enabled = false;
            tbEntry.Text = "Refreshing...";
            lbResults.Items.Clear();
        }

        public void EndRefresh()
        {
            tbEntry.Text = "";
            tbEntry.Enabled = true;
            lbResults.Enabled = true;
            tbEntry.Focus();
        }

        private void tmRefresh_Tick(object sender, EventArgs e)
        {
            tbDisplay_SetText();
        }
    }
}
