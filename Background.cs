using System;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsInput;

namespace Automato
{
    public partial class Background : Form
    {
        readonly InputSimulator sim = new();

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public Background()
        {
            InitializeComponent();
            NativeMethods.RegisterHotKey(this.Handle, 0, (int)KeyModifier.Control, Keys.F10.GetHashCode());
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */

                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                //int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                Debug.WriteLine("SULKINYT!");
                notifyIcon1.Dispose();
                Environment.Exit(0);
                // do something
            }
        }

        private void Background_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("BACKGROUND AUKIII");
            notifyIcon1.Icon = Properties.Resources.app;

            tray_Exit.Click += new System.EventHandler(this.Tray_Exit_Click);
            tray_Openfile.Click += new System.EventHandler(this.Tray_Openfile_Click);
        }

        private void Tray_Exit_Click(object sender, System.EventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void Tray_Openfile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openfiledialog = new();
            Invoke((Action)(() => { openfiledialog.ShowDialog(); }));
            MessageBox.Show(openfiledialog.FileName);
        }

        private void Background_FormClosing(object sender, FormClosingEventArgs e)
        {
            NativeMethods.UnregisterHotKey(this.Handle, 0);
        }
    }
}
