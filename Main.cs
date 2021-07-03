namespace Automato
{
    public partial class Main : Form
    {
        private const int SW_RESTORE = 9;

        readonly InputSimulator sim = new();
        private static readonly string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private string keyfile;
        private readonly bool loop = true;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (!File.Exists(appdir + "\\errorlog.txt"))
            {
                File.WriteAllText("", appdir + "\\errorlog.txt");
            }

            if (File.Exists(@"D:\Omat tiedostot\Documents\Visual Studio 2019\Projects\Automato\Automato\Main.cs"))
            {
                string commandstowrite = "";
                foreach (string item in File.ReadAllLines(@"D:\Omat tiedostot\Documents\Visual Studio 2019\Projects\Automato\Automato\Main.cs"))
                {
                    if (item.Contains("|"))
                    {
                        try
                        {
                            string[] cleancommand = item.Split(new string[] { "StartsWith(\"" }, StringSplitOptions.None);
                            string[] cleancommand2 = cleancommand[1].Split('"');
                            commandstowrite += cleancommand2[0] + Environment.NewLine;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                File.Delete(appdir + "\\commands.txt");
                File.WriteAllText(appdir + "\\commands.txt", commandstowrite);
            }

            string[] args = Environment.GetCommandLineArgs();

            keyfile = appdir + @"\keys.akp";
            if (!File.Exists(keyfile))
            {
                File.WriteAllText(keyfile, "");
            }

            new Thread(() => new Background().ShowDialog()).Start();

            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (File.Exists(args[i]) && Path.GetExtension(args[i]) == ".akp")
                    {
                        keyfile = args[i];
                        Start(loop, keyfile, sim);
                    }
                }
            }
        }

        public static void Start(bool loop, string keyfile, InputSimulator sim)
        {
            while (loop)
            {
                int counter = 0;

                foreach (string line in File.ReadAllLines(keyfile))
                {
                    counter++;
                    string[] splitline = line.Split('|');
                    int count = splitline.Length - 1;

                    if (line.Length > 0)
                    {
                        //Debug.WriteLine(counter);
                        if (counter == 1 && line.StartsWith("LOOP", StringComparison.CurrentCulture))
                        {
                            loop = true;
                            continue;
                        }
                        else if (counter == 1 && !line.StartsWith("LOOP", StringComparison.CurrentCulture))
                        {
                            loop = false;
                        }

                        if (line.StartsWith("PRESSKEY|", StringComparison.CurrentCulture))
                        {
                            sim.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(splitline[1]));
                        }
                        else if (line.StartsWith("TYPE|", StringComparison.CurrentCulture))
                        {
                            sim.Keyboard.TextEntry(splitline[1]);
                        }
                        else if (line.StartsWith("SENDKEYS|", StringComparison.CurrentCulture))
                        {
                            if (count == 2)
                            {
                                sim.Keyboard.ModifiedKeyStroke(KeyCodeConverter.ConvertToVirtualKey(splitline[1]), KeyCodeConverter.ConvertToVirtualKey(splitline[2]));
                            }
                            else if (count == 3)
                            {
                                sim.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(splitline[1]));
                                sim.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(splitline[2]));
                                sim.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(splitline[3]));
                                sim.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(splitline[1]));
                                sim.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(splitline[2]));
                            }
                        }
                        else if (line.StartsWith("WAIT|", StringComparison.CurrentCulture))
                        {
                            bool isNumeric = int.TryParse(splitline[1], out int n);
                            if (isNumeric)
                            {
                                Debug.WriteLine("Waiting for " + n / 1000);
                                Thread.Sleep(n);
                            }
                        }
                        else if (line.StartsWith("RUNCOMMAND|", StringComparison.CurrentCulture))
                        {
                            string runfile = "";

                            if (File.Exists(appdir + @"\" + splitline[1]))
                            {
                                runfile = appdir + @"\" + splitline[1];
                            }
                            else if (File.Exists(splitline[1]))
                            {
                                runfile = splitline[1];
                            }

                            if (runfile.Length > 0)
                            {
                                ProcessStartInfo startInfo = new(runfile);
                                if (count == 2)
                                {
                                    //startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                                    startInfo.Arguments = splitline[2];
                                }
                                Process.Start(startInfo);
                            }
                        }
                        else if (line.StartsWith("WINACTIVATE|", StringComparison.CurrentCulture))
                        {
                            Process[] prc = Process.GetProcessesByName(splitline[1]);
                            if (prc.Length > 0)
                            {
                                if (NativeMethods.IsIconic(prc[0].MainWindowHandle))
                                {
                                    NativeMethods.ShowWindow(prc[0].MainWindowHandle, SW_RESTORE);
                                }
                                NativeMethods.SetForegroundWindow(prc[0].MainWindowHandle);

                                for (int i = 0; i < 5; i++)
                                {
                                    if (NativeMethods.GetForegroundWindow().Equals(prc[0].MainWindowHandle))
                                    {
                                        break;
                                    }
                                    Debug.WriteLine("Waiting window to activate...");
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        else if (line.StartsWith("WAITWIN|", StringComparison.CurrentCulture))
                        {
                            if (count == 2)
                            {
                                for (int i = 0; i < int.Parse(splitline[1], CultureInfo.CurrentCulture) / 1000; i++)
                                {
                                    Process[] prc = Process.GetProcessesByName(splitline[2]);
                                    if (prc.Length > 0)
                                    {
                                        if (NativeMethods.GetForegroundWindow().Equals(prc[0].MainWindowHandle))
                                        {
                                            break;
                                        }
                                        Debug.WriteLine("Waiting executable...");
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        else if (line.StartsWith("WAITFILE|", StringComparison.CurrentCulture))
                        {
                            if (count == 2)
                            {
                                for (int i = 0; i < int.Parse(splitline[1], CultureInfo.CurrentCulture) / 1000; i++)
                                {
                                    if (File.Exists(splitline[2]))
                                    {
                                        break;
                                    }
                                    Debug.WriteLine("Waiting file to exist...");
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        else if (line.StartsWith("MOUSECLICK|", StringComparison.CurrentCulture))
                        {
                            if (count >= 3)
                            {
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(splitline[2]), MouseHelpers.GetMouseY(splitline[3]));
                                Thread.Sleep(10);

                                int clicktimes = 1;

                                if (count == 4)
                                {
                                    clicktimes = int.Parse(splitline[4], CultureInfo.CurrentCulture);
                                }

                                Debug.WriteLine(clicktimes);

                                for (int i = 0; i < clicktimes; i++)
                                {
                                    if (splitline[1].ToLower(CultureInfo.CurrentCulture) == "mouse1")
                                    {
                                        sim.Mouse.LeftButtonClick();
                                    }
                                    else if (splitline[1].ToLower(CultureInfo.CurrentCulture) == "mouse1double")
                                    {
                                        sim.Mouse.LeftButtonDoubleClick();
                                    }
                                    else if (splitline[1].ToLower(CultureInfo.CurrentCulture) == "mouse2")
                                    {
                                        sim.Mouse.RightButtonClick();
                                    }
                                    else if (splitline[1].ToLower(CultureInfo.CurrentCulture) == "mouse2double")
                                    {
                                        sim.Mouse.RightButtonDoubleClick();
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        else if (line.StartsWith("MOUSEMOVE|", StringComparison.CurrentCulture))
                        {
                            if (count == 2)
                            {
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(splitline[1]), MouseHelpers.GetMouseY(splitline[2]));
                            }
                        }
                        else if (line.StartsWith("MOUSEHOLD|", StringComparison.CurrentCulture))
                        {
                            if (count == 3)
                            {
                                bool isNumeric = int.TryParse(splitline[3], out int n);

                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(splitline[1]), MouseHelpers.GetMouseY(splitline[2]));
                                sim.Mouse.LeftButtonDown();
                                if (isNumeric)
                                {
                                    Thread.Sleep(n);
                                }
                                sim.Mouse.LeftButtonUp();
                            }
                        }
                        else if (line.StartsWith("FINDIMAGE|", StringComparison.CurrentCulture))
                        {
                            if (count >= 1)
                            {
                                int windowwidth = SystemInformation.VirtualScreen.Width,
                                    windowheight = SystemInformation.VirtualScreen.Height,
                                    windowx = SystemInformation.VirtualScreen.Left,
                                    windowy = SystemInformation.VirtualScreen.Top;
                                Size windowsize = new(windowwidth, windowheight);

                                Rectangle WindowBounds = new();

                                if (count == 3)
                                {
                                    Process[] prc = Process.GetProcessesByName(splitline[3]);
                                    if (prc.Length > 0)
                                    {
                                        if (NativeMethods.GetWindowRect(prc[0].MainWindowHandle, out var rct))
                                        {
                                            WindowBounds.X = rct.Left;
                                            WindowBounds.Y = rct.Top;
                                            WindowBounds.Width = rct.Right - rct.Left + 1;
                                            WindowBounds.Height = rct.Bottom - rct.Top + 1;

                                            windowwidth = WindowBounds.Width;
                                            windowheight = WindowBounds.Height;
                                            windowx = WindowBounds.X;
                                            windowy = WindowBounds.Y;
                                            windowsize = new Size(windowwidth, windowheight);
                                        }
                                    }
                                }

                                Debug.WriteLine(windowwidth + " " + windowheight + " " + windowx + " " + windowy);

                                ////// FOR SAVED IMAGE ////////
                                Image imagefile = Image.FromFile(splitline[1]);
                                Bitmap bitmapfile = new(imagefile);
                                //////////////////////////////

                                Bitmap bmpScreenshot = new(windowwidth, windowheight, PixelFormat.Format32bppArgb);

                                // Create a graphics object from the bitmap.
                                Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                                // Take the screenshot from the upper left corner to the right bottom corner.
                                Debug.WriteLine(windowsize.ToString());
                                gfxScreenshot.CopyFromScreen(windowx, windowy, 0, 0, windowsize, CopyPixelOperation.SourceCopy);
                                //bmpScreenshot.Save("kuva.bmp", ImageFormat.Bmp);
                                //Environment.Exit(0);
                                Point? picturefound = ImageService.FindImageInImage(bmpScreenshot, bitmapfile);

                                if (picturefound != null)
                                {
                                    Debug.WriteLine("HasValue");
                                    if (count >= 2)
                                    {
                                        Debug.WriteLine("count == 2");
                                        if (splitline[2] == "CLICK")
                                        {
                                            double XCoord = picturefound.Value.X + (bitmapfile.Width / 2);
                                            double YCoord = picturefound.Value.Y + (bitmapfile.Height / 2);
                                            sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(XCoord.ToString(CultureInfo.CurrentCulture)), MouseHelpers.GetMouseY(YCoord.ToString(CultureInfo.CurrentCulture)));
                                            Thread.Sleep(100);
                                            sim.Mouse.LeftButtonClick();
                                            Thread.Sleep(100);
                                        }
                                    }
                                }

                                bmpScreenshot.Dispose();
                                bitmapfile.Dispose();
                            }
                        }
                        else if (line.StartsWith("WAITCLOCK|", StringComparison.CurrentCulture))
                        {
                            if (count == 1)
                            {
                                bool wait = true;

                                while (wait)
                                {
                                    DateTime now = DateTime.Now;

                                    if (splitline[1] == now.ToString("HH") + ":" + now.ToString("mm"))
                                    {
                                        wait = false;
                                    }

                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        else if (line.StartsWith("CHECKPIXEL|", StringComparison.CurrentCulture))
                        {
                            if (count >= 5)
                            {
                                int windowwidth = SystemInformation.VirtualScreen.Width,
                                    windowheight = SystemInformation.VirtualScreen.Height,
                                    windowx = SystemInformation.VirtualScreen.Left,
                                    windowy = SystemInformation.VirtualScreen.Top;
                                Size windowsize = new(windowwidth, windowheight);

                                //Debug.WriteLine(windowwidth + " " + windowheight + " " + windowx + " " + windowy);



                                // Take the screenshot from the upper left corner to the right bottom corner.

                                try
                                {
                                    Bitmap bmpScreenshot = new(windowwidth, windowheight, PixelFormat.Format32bppRgb);
                                    Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                                    gfxScreenshot.CopyFromScreen(windowx, windowy, 0, 0, windowsize, CopyPixelOperation.SourceCopy);


                                    //bmpScreenshot.Save("testi.bmp", ImageFormat.Bmp);

                                    var x = Convert.ToInt32(splitline[1]);
                                    var y = Convert.ToInt32(splitline[2]);

                                    var r = Convert.ToInt32(splitline[3]);
                                    var g = Convert.ToInt32(splitline[4]);
                                    var b = Convert.ToInt32(splitline[5]);

                                    //bmpScreenshot.SetPixel(x, y, Color.Red);
                                    //bmpScreenshot.Save("test.bmp", ImageFormat.Bmp);

                                    var test2 = bmpScreenshot.GetPixel(x, y);

                                    if (test2.R == r && test2.G == g && test2.B == b)
                                    {
                                        sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(x.ToString(CultureInfo.CurrentCulture)), MouseHelpers.GetMouseY(y.ToString(CultureInfo.CurrentCulture)));
                                        Thread.Sleep(10);
                                        sim.Mouse.LeftButtonClick();
                                    }

                                    gfxScreenshot.Dispose();
                                    bmpScreenshot.Dispose();
                                }
                                catch
                                {
                                }

                            }
                        }
                        else if (line.StartsWith("STOP", StringComparison.CurrentCulture))
                        {
                            break;
                        }
                    }
                }
            }
            Debug.WriteLine("Sulki");
            Application.Exit();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
            //UnregisterHotKey(this.Handle, 0);
        }
    }
}
