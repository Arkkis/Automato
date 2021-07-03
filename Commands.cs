namespace Automato
{
    class Commands
    {
        private const int SW_RESTORE = 9;

        public static void WinActivate(string argument1)
        {
            var prc = Process.GetProcessesByName(argument1);

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

        public static void MouseHold(InputSimulator input, string argument1, string argument2, string argument3)
        {
            var isNumeric = int.TryParse(argument3, out int n);

            input.Mouse.MoveMouseToPositionOnVirtualDesktop(MouseHelpers.GetMouseX(argument1), MouseHelpers.GetMouseY(argument2));
            input.Mouse.LeftButtonDown();
            if (isNumeric)
            {
                Thread.Sleep(n);
            }
            input.Mouse.LeftButtonUp();
        }
    }
}
