namespace Automato
{
    public partial class Background : Form
    {
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
                Debug.WriteLine("SULKINYT!");
                notifyIcon1.Dispose();
                Environment.Exit(0);
            }
        }

        private void Background_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("BACKGROUND AUKIII");
            notifyIcon1.Icon = Properties.Resources.app;

            tray_Exit.Click += new EventHandler(Tray_Exit_Click);
            tray_Openfile.Click += new EventHandler(Tray_Openfile_Click);
        }

        private void Tray_Exit_Click(object sender, EventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void Tray_Openfile_Click(object sender, EventArgs e)
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
