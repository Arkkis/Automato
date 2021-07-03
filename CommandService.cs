using System.Reflection;

namespace Automato
{
    class CommandService
    {
        private const int SW_RESTORE = 9;
        private readonly string[] _arguments;
        private readonly InputSimulator _input;
        private readonly string applicationPath = Assembly.GetEntryAssembly()?.Location ?? "";

        public CommandService(string[] arguments, InputSimulator input)
        {
            _arguments = arguments;
            _input = input;
        }

        private (string, string, string, string) Arguments()
        {
            string argument1;
            string argument2 = string.Empty;
            string argument3 = string.Empty;
            string argument4 = string.Empty;
            
            if (_arguments.Length >= 1)
            {
                argument1 = _arguments[1];
            }
            else
            {
                throw new Exception("No arguments for command");
            }

            if (_arguments.Length >= 2)
            {
                argument2 = _arguments[2];
            }

            if (_arguments.Length >= 3)
            {
                argument3 = _arguments[3];
            }

            if (_arguments.Length >= 4)
            {
                argument4 = _arguments[4];
            }

            return (argument1, argument2, argument3, argument4);
        }

        public void WinActivate()
        {
            var (processName, _, _, _) = Arguments();

            var prc = Process.GetProcessesByName(processName);

            if (prc.Length == 0)
            {
                return;
            }

            if (NativeMethods.IsIconic(prc[0].MainWindowHandle))
            {
                NativeMethods.ShowWindow(prc[0].MainWindowHandle, SW_RESTORE);
            }

            NativeMethods.SetForegroundWindow(prc[0].MainWindowHandle);

            for (var i = 0; i < 5; i++)
            {
                if (NativeMethods.GetForegroundWindow().Equals(prc[0].MainWindowHandle))
                {
                    break;
                }
                Debug.WriteLine("Waiting window to activate...");
                Thread.Sleep(1000);
            }
        }

        public void MouseHold()
        {
            var (holdInMilliseconds, x, y, _) = Arguments();

            var isNumeric = int.TryParse(holdInMilliseconds, out var holdTime);

            _input.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(x), MouseHelpers.GetMouseY(y));
            _input.Mouse.LeftButtonDown();

            if (isNumeric)
            {
                Thread.Sleep(holdTime);
            }

            _input.Mouse.LeftButtonUp();
        }

        public void Wait()
        {
            var (waitTime, _, _, _) = Arguments();

            var isNumeric = int.TryParse(waitTime, out int seconds);

            if (isNumeric)
            {
                Debug.WriteLine("Waiting for " + seconds / 1000);
                Thread.Sleep(seconds);
            }
        }

        public void SendKeys()
        {
            var (modifierKey1, key1, key2, _) = Arguments();

            if (!string.IsNullOrEmpty(key1))
            {
                _input.Keyboard.ModifiedKeyStroke(KeyCodeConverter.ConvertToVirtualKey(modifierKey1), KeyCodeConverter.ConvertToVirtualKey(key1));
            }
            else if (!string.IsNullOrEmpty(key2))
            {
                _input.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(modifierKey1));
                _input.Keyboard.KeyDown(KeyCodeConverter.ConvertToVirtualKey(key1));
                _input.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(key2));
                _input.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(modifierKey1));
                _input.Keyboard.KeyUp(KeyCodeConverter.ConvertToVirtualKey(key1));
            }
        }

        public void Type()
        {
            var (text, _, _, _) = Arguments();

            _input.Keyboard.TextEntry(text);
        }

        public void PressKey()
        {
            var (key, _, _, _) = Arguments();

            _input.Keyboard.KeyPress(KeyCodeConverter.ConvertToVirtualKey(key));
        }

        public void RunCommand()
        {
            var (filePath, launchArguments, _, _) = Arguments();

            var runfile = string.Empty;

            var fileInSamePath = $"{applicationPath}\\{filePath}";

            if (File.Exists(fileInSamePath))
            {
                runfile = fileInSamePath;
            }
            else if (File.Exists(filePath))
            {
                runfile = filePath;
            }

            var startInfo = new ProcessStartInfo(runfile);

            if (!string.IsNullOrEmpty(launchArguments))
            {
                startInfo.Arguments = launchArguments;
            }

            Process.Start(startInfo);
        }

        public void WaitWindowToBeActive()
        {
            var (waitTimeInSeconds, processName, _, _) = Arguments();

            if (!string.IsNullOrEmpty(processName))
            {
                for (int i = 0; i < int.Parse(waitTimeInSeconds, CultureInfo.CurrentCulture) / 1000; i++)
                {
                    var prc = Process.GetProcessesByName(processName);
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
    }
}
