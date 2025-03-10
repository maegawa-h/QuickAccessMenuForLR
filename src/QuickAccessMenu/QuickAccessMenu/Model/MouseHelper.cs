using System.Runtime.InteropServices;

namespace QuickAccessMenu.Extensions.Model
{
    class MouseHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static POINT GetMousePosition()
        {
            GetCursorPos(out POINT point);
            return point;
        }
    }
}
