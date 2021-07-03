namespace Automato
{
    public partial class Main : Form
    {
        private static readonly string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        //private string keyfile;
        private bool loop = false;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            var keyfile = $"{applicationPath}\\keys.akp";

            if (!File.Exists(keyfile))
            {
                File.WriteAllText(keyfile, "");
            }

            new Thread(() => new Background().ShowDialog()).Start();

            if (args.Length > 1)
            {
                var sim = new InputSimulator();

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
            var commandList = File.ReadAllLines(keyfile);

            while (loop)
            {
                var counter = 0;

                foreach (string commandLine in commandList)
                {
                    counter++;
                    var arguments = commandLine.Split('|');
                    var count = arguments.Length - 1;

                    if (commandLine.Length == 0)
                    {
                        throw new Exception("Invalid command");
                    }

                    var command = arguments[0];

                    switch (command)
                    {
                        case "LOOP":
                            if (counter == 1)
                            {
                                loop = true;
                                continue;
                            }
                            break;

                        case "PRESSKEY":
                            sim.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(arguments[1]));
                            break;

                        case "TYPE":
                            sim.Keyboard.TextEntry(arguments[1]);
                            break;

                        case "SENDKEYS":
                            if (count == 2)
                            {
                                sim.Keyboard.ModifiedKeyStroke(KeyCodeConverter.ConvertToVirtualKey(arguments[1]), KeyCodeConverter.ConvertToVirtualKey(arguments[2]));
                            }
                            else if (count == 3)
                            {
                                sim.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(arguments[1]));
                                sim.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(arguments[2]));
                                sim.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(arguments[3]));
                                sim.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(arguments[1]));
                                sim.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(arguments[2]));
                            }
                            break;

                        case "WAIT":
                            var isNumeric = int.TryParse(arguments[1], out int seconds);
                            if (isNumeric)
                            {
                                Debug.WriteLine("Waiting for " + seconds / 1000);
                                Thread.Sleep(seconds);
                            }
                            break;

                        case "RUNCOMMAND":
                            var runfile = string.Empty;

                            if (File.Exists(applicationPath + @"\" + arguments[1]))
                            {
                                runfile = applicationPath + @"\" + arguments[1];
                            }
                            else if (File.Exists(arguments[1]))
                            {
                                runfile = arguments[1];
                            }

                            if (runfile.Length > 0)
                            {
                                ProcessStartInfo startInfo = new(runfile);
                                if (count == 2)
                                {
                                    //startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                                    startInfo.Arguments = arguments[2];
                                }
                                Process.Start(startInfo);
                            }
                            break;

                        case "WINACTIVATE":
                            Commands.WinActivate(arguments[1]);
                            break;

                        case "WAITWIN":
                            if (count == 2)
                            {
                                for (int i = 0; i < int.Parse(arguments[1], CultureInfo.CurrentCulture) / 1000; i++)
                                {
                                    var prc = Process.GetProcessesByName(arguments[2]);
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
                            break;

                        case "WAITFILE":
                            if (count == 2)
                            {
                                for (int i = 0; i < int.Parse(arguments[1], CultureInfo.CurrentCulture) / 1000; i++)
                                {
                                    if (File.Exists(arguments[2]))
                                    {
                                        break;
                                    }
                                    Debug.WriteLine("Waiting file to exist...");
                                    Thread.Sleep(1000);
                                }
                            }
                            break;

                        case "MOUSECLICK":
                            if (count >= 3)
                            {
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(arguments[2]), MouseHelpers.GetMouseY(arguments[3]));
                                Thread.Sleep(10);

                                int clicktimes = 1;

                                if (count == 4)
                                {
                                    clicktimes = int.Parse(arguments[4], CultureInfo.CurrentCulture);
                                }

                                Debug.WriteLine(clicktimes);

                                for (int i = 0; i < clicktimes; i++)
                                {
                                    if (arguments[1].ToLower(CultureInfo.CurrentCulture) == "mouse1")
                                    {
                                        sim.Mouse.LeftButtonClick();
                                    }
                                    else if (arguments[1].ToLower(CultureInfo.CurrentCulture) == "mouse1double")
                                    {
                                        sim.Mouse.LeftButtonDoubleClick();
                                    }
                                    else if (arguments[1].ToLower(CultureInfo.CurrentCulture) == "mouse2")
                                    {
                                        sim.Mouse.RightButtonClick();
                                    }
                                    else if (arguments[1].ToLower(CultureInfo.CurrentCulture) == "mouse2double")
                                    {
                                        sim.Mouse.RightButtonDoubleClick();
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                            break;

                        case "MOUSEMOVE":
                            if (count == 2)
                            {
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(arguments[1]), MouseHelpers.GetMouseY(arguments[2]));
                            }
                            break;

                        case "MOUSEHOLD":
                            
                            break;

                        case "FINDIMAGE":
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
                                    var prc = Process.GetProcessesByName(arguments[3]);
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
                                Image imagefile = Image.FromFile(arguments[1]);
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
                                        if (arguments[2] == "CLICK")
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
                            break;

                        case "WAITCLOCK":
                            if (count == 1)
                            {
                                bool wait = true;

                                while (wait)
                                {
                                    DateTime now = DateTime.Now;

                                    if (arguments[1] == now.ToString("HH") + ":" + now.ToString("mm"))
                                    {
                                        wait = false;
                                    }

                                    Thread.Sleep(1000);
                                }
                            }
                            break;

                        case "CHECKPIXEL":
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

                                    var x = Convert.ToInt32(arguments[1]);
                                    var y = Convert.ToInt32(arguments[2]);

                                    var r = Convert.ToInt32(arguments[3]);
                                    var g = Convert.ToInt32(arguments[4]);
                                    var b = Convert.ToInt32(arguments[5]);

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
                            break;

                        case "STOP":
                            break;

                        default:
                            break;
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
