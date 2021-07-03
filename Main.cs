using System;
using System.Windows.Forms;
using WindowsInput.Native;
using WindowsInput;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace Automato
{
    public partial class Main : Form
    {
        //private Background BackgroundForm;
        //private bool StopApp = false;

        private const int SW_RESTORE = 9;

        readonly InputSimulator sim = new();
        private static readonly string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private string keyfile;
        private bool loop = true;
        private static WebClient myWebClient = new();

        public Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

            FileInfo hufile = new(appdir + "\\" + "AutomatoUpdater.exe");
            if (!File.Exists(appdir + "\\" + "AutomatoUpdater.exe") || hufile.Length < 7680)
            {
                KillProcess("AutomatoUpdater", 3000);

                myWebClient = new WebClient();
                Uri uri = new("");
                myWebClient.DownloadFileAsync(uri, "AutomatoUpdater.exe");
            }

            string[] args = Environment.GetCommandLineArgs();
            bool skipupdate = false;

            keyfile = appdir + @"\keys.akp";
            if (!File.Exists(keyfile))
            {
                File.WriteAllText(keyfile, "");
            }

            if (skipupdate == false)
            {
                if (GetLatestVersion() != "noconnection" && GetLatestVersion() != Text.Replace("Automato (", "").Replace(")", "") && File.Exists("AutomatoUpdater.exe"))
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "AutomatoUpdater.exe",
                        Arguments = "/version=" + Text.Replace("Automato (", "").Replace(")", "")
                    };
                    Process.Start(startInfo);
                    Application.Exit();
                }
            }

            //BackgroundForm = new Background();
            //BackgroundForm.Show();
            new Thread(() => new Background().ShowDialog()).Start();

            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToString(CultureInfo.CurrentCulture) == "-noupdate")
                    {
                        skipupdate = true;
                    }

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
                            sim.Keyboard.KeyPress(ConvertToVirtualKey(splitline[1]));
                        }
                        else if (line.StartsWith("TYPE|", StringComparison.CurrentCulture))
                        {
                            sim.Keyboard.TextEntry(splitline[1]);
                        }
                        else if (line.StartsWith("SENDKEYS|", StringComparison.CurrentCulture))
                        {
                            if (count == 2)
                            {
                                sim.Keyboard.ModifiedKeyStroke(ConvertToVirtualKey(splitline[1]), ConvertToVirtualKey(splitline[2]));
                            }
                            else if (count == 3)
                            {
                                sim.Keyboard.KeyDown(ConvertToVirtualKey(splitline[1]));
                                sim.Keyboard.KeyDown(ConvertToVirtualKey(splitline[2]));
                                sim.Keyboard.KeyPress(ConvertToVirtualKey(splitline[3]));
                                sim.Keyboard.KeyUp(ConvertToVirtualKey(splitline[1]));
                                sim.Keyboard.KeyUp(ConvertToVirtualKey(splitline[2]));
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
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(GetScreenXCoordinateForMouse(splitline[2]), GetScreenYCoordinateForMouse(splitline[3]));
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
                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(GetScreenXCoordinateForMouse(splitline[1]), GetScreenYCoordinateForMouse(splitline[2]));
                            }
                        }
                        else if (line.StartsWith("MOUSEHOLD|", StringComparison.CurrentCulture))
                        {
                            if (count == 3)
                            {
                                bool isNumeric = int.TryParse(splitline[3], out int n);

                                sim.Mouse.MoveMouseToPositionOnVirtualDesktop(GetScreenXCoordinateForMouse(splitline[1]), GetScreenYCoordinateForMouse(splitline[2]));
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
                                        if (NativeMethods.GetWindowRect(prc[0].MainWindowHandle, out RECT rct))
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
                                Point? picturefound = Find(bmpScreenshot, bitmapfile);

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
                                            sim.Mouse.MoveMouseToPositionOnVirtualDesktop(GetScreenXCoordinateForMouse(XCoord.ToString(CultureInfo.CurrentCulture)), GetScreenYCoordinateForMouse(YCoord.ToString(CultureInfo.CurrentCulture)));
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
                                        sim.Mouse.MoveMouseToPositionOnVirtualDesktop(GetScreenXCoordinateForMouse(x.ToString(CultureInfo.CurrentCulture)), GetScreenYCoordinateForMouse(y.ToString(CultureInfo.CurrentCulture)));
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

        private static double GetScreenXCoordinateForMouse(string x)
        {
            double x1 = double.Parse(x, CultureInfo.CurrentCulture);
            double x2 = x1 / (SystemInformation.VirtualScreen.Width);
            x1 = 65535 * x2;

            return Math.Round(x1, 0);
        }

        private static double GetScreenYCoordinateForMouse(string y)
        {
            double y1 = double.Parse(y, CultureInfo.CurrentCulture);
            double y2 = y1 / SystemInformation.VirtualScreen.Height;
            y1 = 65535 * y2;

            return Math.Round(y1, 0);
        }

        private static VirtualKeyCode ConvertToVirtualKey(string key)
        {
            VirtualKeyCode virtualkey = VirtualKeyCode.LMENU;

            if (key.ToLower(CultureInfo.CurrentCulture) == "a")
            {
                virtualkey = VirtualKeyCode.VK_A;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "b")
            {
                virtualkey = VirtualKeyCode.VK_B;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "c")
            {
                virtualkey = VirtualKeyCode.VK_C;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "d")
            {
                virtualkey = VirtualKeyCode.VK_D;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "e")
            {
                virtualkey = VirtualKeyCode.VK_E;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f")
            {
                virtualkey = VirtualKeyCode.VK_F;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "g")
            {
                virtualkey = VirtualKeyCode.VK_G;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "h")
            {
                virtualkey = VirtualKeyCode.VK_H;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "i")
            {
                virtualkey = VirtualKeyCode.VK_I;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "j")
            {
                virtualkey = VirtualKeyCode.VK_J;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "k")
            {
                virtualkey = VirtualKeyCode.VK_K;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "l")
            {
                virtualkey = VirtualKeyCode.VK_L;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "m")
            {
                virtualkey = VirtualKeyCode.VK_M;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "n")
            {
                virtualkey = VirtualKeyCode.VK_N;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "o")
            {
                virtualkey = VirtualKeyCode.VK_O;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "p")
            {
                virtualkey = VirtualKeyCode.VK_P;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "q")
            {
                virtualkey = VirtualKeyCode.VK_Q;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "r")
            {
                virtualkey = VirtualKeyCode.VK_R;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "s")
            {
                virtualkey = VirtualKeyCode.VK_S;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "t")
            {
                virtualkey = VirtualKeyCode.VK_T;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "u")
            {
                virtualkey = VirtualKeyCode.VK_U;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "v")
            {
                virtualkey = VirtualKeyCode.VK_V;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "w")
            {
                virtualkey = VirtualKeyCode.VK_W;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "x")
            {
                virtualkey = VirtualKeyCode.VK_X;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "y")
            {
                virtualkey = VirtualKeyCode.VK_Y;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "z")
            {
                virtualkey = VirtualKeyCode.VK_Z;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "enter")
            {
                virtualkey = VirtualKeyCode.RETURN;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "lalt")
            {
                virtualkey = VirtualKeyCode.MENU;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "ralt")
            {
                virtualkey = VirtualKeyCode.RMENU;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "lcontrol")
            {
                virtualkey = VirtualKeyCode.CONTROL;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "rcontrol")
            {
                virtualkey = VirtualKeyCode.RCONTROL;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "lshift")
            {
                virtualkey = VirtualKeyCode.LSHIFT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "rshift")
            {
                virtualkey = VirtualKeyCode.RSHIFT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "lwin")
            {
                virtualkey = VirtualKeyCode.LWIN;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "rwin")
            {
                virtualkey = VirtualKeyCode.RWIN;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "delete")
            {
                virtualkey = VirtualKeyCode.DELETE;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "tab")
            {
                virtualkey = VirtualKeyCode.TAB;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "capslock")
            {
                virtualkey = VirtualKeyCode.CAPITAL;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "pause")
            {
                virtualkey = VirtualKeyCode.PAUSE;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "backspace")
            {
                virtualkey = VirtualKeyCode.BACK;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "space")
            {
                virtualkey = VirtualKeyCode.SPACE;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "pageup")
            {
                virtualkey = VirtualKeyCode.PRIOR;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "pagedown")
            {
                virtualkey = VirtualKeyCode.NEXT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "end")
            {
                virtualkey = VirtualKeyCode.END;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "home")
            {
                virtualkey = VirtualKeyCode.HOME;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "left")
            {
                virtualkey = VirtualKeyCode.LEFT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "right")
            {
                virtualkey = VirtualKeyCode.RIGHT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "up")
            {
                virtualkey = VirtualKeyCode.UP;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "down")
            {
                virtualkey = VirtualKeyCode.DOWN;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "printscreen")
            {
                virtualkey = VirtualKeyCode.SNAPSHOT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "insert")
            {
                virtualkey = VirtualKeyCode.INSERT;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "0")
            {
                virtualkey = VirtualKeyCode.VK_0;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "1")
            {
                virtualkey = VirtualKeyCode.VK_1;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "2")
            {
                virtualkey = VirtualKeyCode.VK_2;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "3")
            {
                virtualkey = VirtualKeyCode.VK_3;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "4")
            {
                virtualkey = VirtualKeyCode.VK_4;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "5")
            {
                virtualkey = VirtualKeyCode.VK_5;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "6")
            {
                virtualkey = VirtualKeyCode.VK_6;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "7")
            {
                virtualkey = VirtualKeyCode.VK_7;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "8")
            {
                virtualkey = VirtualKeyCode.VK_8;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "9")
            {
                virtualkey = VirtualKeyCode.VK_9;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "numlock")
            {
                virtualkey = VirtualKeyCode.NUMLOCK;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num0")
            {
                virtualkey = VirtualKeyCode.NUMPAD0;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num1")
            {
                virtualkey = VirtualKeyCode.NUMPAD1;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num2")
            {
                virtualkey = VirtualKeyCode.NUMPAD2;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num3")
            {
                virtualkey = VirtualKeyCode.NUMPAD3;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num4")
            {
                virtualkey = VirtualKeyCode.NUMPAD4;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num5")
            {
                virtualkey = VirtualKeyCode.NUMPAD5;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num6")
            {
                virtualkey = VirtualKeyCode.NUMPAD6;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num7")
            {
                virtualkey = VirtualKeyCode.NUMPAD7;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num8")
            {
                virtualkey = VirtualKeyCode.NUMPAD8;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "num9")
            {
                virtualkey = VirtualKeyCode.NUMPAD9;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "menu")
            {
                virtualkey = VirtualKeyCode.APPS;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f1")
            {
                virtualkey = VirtualKeyCode.F1;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f2")
            {
                virtualkey = VirtualKeyCode.F2;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f3")
            {
                virtualkey = VirtualKeyCode.F3;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f4")
            {
                virtualkey = VirtualKeyCode.F4;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f5")
            {
                virtualkey = VirtualKeyCode.F5;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f6")
            {
                virtualkey = VirtualKeyCode.F6;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f7")
            {
                virtualkey = VirtualKeyCode.F7;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f8")
            {
                virtualkey = VirtualKeyCode.F8;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f9")
            {
                virtualkey = VirtualKeyCode.F9;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f10")
            {
                virtualkey = VirtualKeyCode.F10;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f11")
            {
                virtualkey = VirtualKeyCode.F11;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "f12")
            {
                virtualkey = VirtualKeyCode.F12;
            }
            else if (key.ToLower(CultureInfo.CurrentCulture) == "mouse1")
            {
                virtualkey = VirtualKeyCode.F12;
            }

            return virtualkey;
        }

        private static string GetLatestVersion()
        {
            try
            {
                Uri urlAddress = new("osoite");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                request.Timeout = 2000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();

                    var readStream = response.CharacterSet == null ? new StreamReader(receiveStream ?? throw new InvalidOperationException()) : new StreamReader(receiveStream ?? throw new InvalidOperationException(), Encoding.GetEncoding(response.CharacterSet));

                    string data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();

                    return data;
                }
                else
                {
                    return "";
                }
            }
            catch (WebException)
            {
                return "noconnection";
            }
        }

        private async void KillProcess(string process, int sleeptime)
        {
            Process[] processes = Process.GetProcessesByName(process);
            if (processes.Length > 0)
            {
                try
                {
                    processes[0].Kill();
                    await PutTaskDelay(sleeptime).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                    // ignored
                }
            }
        }

        private static async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay).ConfigureAwait(false);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
            //UnregisterHotKey(this.Handle, 0);
        }

        static void ReportError(string error)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(appdir + "\\" + "errorlog.txt"))
                {
                    sw.WriteLine(error + Environment.NewLine);
                }
                WebClient client = new();
                client.OpenRead("osoite" + GetLatestVersion() + " - " + error.Replace(Environment.NewLine, "*--*"));
                client.Dispose();
                MessageBox.Show(Properties.Resources.string_error);
                Application.Exit();
            }
            catch (WebException)
            {

            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ReportError(e.Exception.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportError(e.ExceptionObject.ToString());
        }

        public static Point? Find(Bitmap haystack, Bitmap needle)
        {
            if (null == haystack || null == needle)
            {
                return null;
            }
            if (haystack.Width < needle.Width || haystack.Height < needle.Height)
            {
                return null;
            }

            var haystackArray = GetPixelArray(haystack);
            var needleArray = GetPixelArray(needle);

            foreach (var firstLineMatchPoint in FindMatch(haystackArray.Take(haystack.Height - needle.Height), needleArray[0]))
            {
                if (IsNeedlePresentAtLocation(haystackArray, needleArray, firstLineMatchPoint, 1))
                {
                    return firstLineMatchPoint;
                }
            }

            return null;
        }

        private static int[][] GetPixelArray(Bitmap bitmap)
        {
            var result = new int[bitmap.Height][];
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            for (int y = 0; y < bitmap.Height; ++y)
            {
                result[y] = new int[bitmap.Width];
                Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, result[y], 0, result[y].Length);
            }

            bitmap.UnlockBits(bitmapData);

            return result;
        }

        private static IEnumerable<Point> FindMatch(IEnumerable<int[]> haystackLines, int[] needleLine)
        {
            var y = 0;
            foreach (var haystackLine in haystackLines)
            {
                for (int x = 0, n = haystackLine.Length - needleLine.Length; x < n; ++x)
                {
                    if (ContainSameElements(haystackLine, x, needleLine, 0, needleLine.Length))
                    {
                        yield return new Point(x, y);
                    }
                }
                y += 1;
            }
        }

        private static bool ContainSameElements(int[] first, int firstStart, int[] second, int secondStart, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                if (first[i + firstStart] != second[i + secondStart])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNeedlePresentAtLocation(int[][] haystack, int[][] needle, Point point, int alreadyVerified)
        {
            //we already know that "alreadyVerified" lines already match, so skip them
            for (int y = alreadyVerified; y < needle.Length; ++y)
            {
                if (!ContainSameElements(haystack[y + point.Y], point.X, needle[y], 0, needle[y].Length))
                {
                    return false;
                }
            }
            return true;
        }
    }

    internal struct RECT
    {
        internal int Left;        // x position of upper-left corner
        internal int Top;         // y position of upper-left corner
        internal int Right;       // x position of lower-right corner
        internal int Bottom;      // y position of lower-right corner
    }

    internal class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }
}
