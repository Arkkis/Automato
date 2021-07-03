namespace Automato
{
    class MouseHelpers
    {
        internal static double GetMouseX(string x)
        {
            double x1 = double.Parse(x, CultureInfo.CurrentCulture);
            double x2 = x1 / (SystemInformation.VirtualScreen.Width);
            x1 = 65535 * x2;

            return Math.Round(x1, 0);
        }

        internal static double GetMouseY(string y)
        {
            double y1 = double.Parse(y, CultureInfo.CurrentCulture);
            double y2 = y1 / SystemInformation.VirtualScreen.Height;
            y1 = 65535 * y2;

            return Math.Round(y1, 0);
        }
    }
}
