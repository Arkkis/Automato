using System;
using System.Windows.Forms;

namespace Snapper
{
    public partial class HotkeyForm : Form
    {
        public HotkeyForm()
        {
            InitializeComponent();
            HotkeysEnable();
        }

        public void HotkeysEnable()
        {
            Main.CaptureClosed = false;
            NativeMethods.RegisterHotKey(this.Handle, 0, (int)KeyModifier.Control, Keys.F1.GetHashCode());
            NativeMethods.RegisterHotKey(this.Handle, 1, (int)KeyModifier.Control, Keys.F2.GetHashCode());
            NativeMethods.RegisterHotKey(this.Handle, 2, (int)KeyModifier.Control, Keys.F3.GetHashCode());
            NativeMethods.RegisterHotKey(this.Handle, 6, (int)KeyModifier.Control, Keys.F4.GetHashCode());
            //NativeMethods.RegisterHotKey(this.Handle, 3, (int)KeyModifier.Control, Keys.F5.GetHashCode());
            NativeMethods.RegisterHotKey(this.Handle, 4, (int)KeyModifier.Control, Keys.F11.GetHashCode());
            NativeMethods.RegisterHotKey(this.Handle, 5, (int)KeyModifier.Control, Keys.F12.GetHashCode());
            //NativeMethods.RegisterHotKey(this.Handle, 7, (int)KeyModifier.Control, Keys.F1.GetHashCode());
        }

        public void HotkeysDisable()
        {
            Main.CaptureClosed = true;
            NativeMethods.UnregisterHotKey(this.Handle, 0);
            NativeMethods.UnregisterHotKey(this.Handle, 1);
            NativeMethods.UnregisterHotKey(this.Handle, 2);
            //NativeMethods.UnregisterHotKey(this.Handle, 3);
            NativeMethods.UnregisterHotKey(this.Handle, 4);
            NativeMethods.UnregisterHotKey(this.Handle, 5);
            NativeMethods.UnregisterHotKey(this.Handle, 6);
            //NativeMethods.UnregisterHotKey(this.Handle, 7);
        }

        private void HotkeyForm_Load(object sender, EventArgs e)
        {
            Hide();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                Main Main = new Main();
                Main.Capture(m);
            }
        }

        private void HotkeyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotkeysDisable();
        }
    }

    enum KeyModifier
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        WinKey = 8
    }
}
