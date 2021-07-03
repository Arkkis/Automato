using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace Automato
{
    internal class KeyCodeConverter
    {
        internal static VirtualKeyCode ConvertToVirtualKey(string key)
        {
            key = key.ToLower();

            var virtualkey = VirtualKeyCode.LMENU;

            switch (key)
            {
                case "a":
                    virtualkey = VirtualKeyCode.VK_A;
                    break;
                case "b":
                    virtualkey = VirtualKeyCode.VK_B;
                    break;
                case "c":
                    virtualkey = VirtualKeyCode.VK_C;
                    break;
                case "d":
                    virtualkey = VirtualKeyCode.VK_D;
                    break;
                case "e":
                    virtualkey = VirtualKeyCode.VK_E;
                    break;
                case "f":
                    virtualkey = VirtualKeyCode.VK_F;
                    break;
                case "g":
                    virtualkey = VirtualKeyCode.VK_G;
                    break;
                case "h":
                    virtualkey = VirtualKeyCode.VK_H;
                    break;
                case "i":
                    virtualkey = VirtualKeyCode.VK_I;
                    break;
                case "j":
                    virtualkey = VirtualKeyCode.VK_J;
                    break;
                case "k":
                    virtualkey = VirtualKeyCode.VK_K;
                    break;
                case "l":
                    virtualkey = VirtualKeyCode.VK_L;
                    break;
                case "m":
                    virtualkey = VirtualKeyCode.VK_M;
                    break;
                case "n":
                    virtualkey = VirtualKeyCode.VK_N;
                    break;
                case "o":
                    virtualkey = VirtualKeyCode.VK_O;
                    break;
                case "p":
                    virtualkey = VirtualKeyCode.VK_P;
                    break;
                case "q":
                    virtualkey = VirtualKeyCode.VK_Q;
                    break;
                case "r":
                    virtualkey = VirtualKeyCode.VK_R;
                    break;
                case "s":
                    virtualkey = VirtualKeyCode.VK_S;
                    break;
                case "t":
                    virtualkey = VirtualKeyCode.VK_T;
                    break;
                case "u":
                    virtualkey = VirtualKeyCode.VK_U;
                    break;
                case "v":
                    virtualkey = VirtualKeyCode.VK_V;
                    break;
                case "w":
                    virtualkey = VirtualKeyCode.VK_W;
                    break;
                case "x":
                    virtualkey = VirtualKeyCode.VK_X;
                    break;
                case "y":
                    virtualkey = VirtualKeyCode.VK_Y;
                    break;
                case "z":
                    virtualkey = VirtualKeyCode.VK_Z;
                    break;
                case "enter":
                    virtualkey = VirtualKeyCode.RETURN;
                    break;
                case "lalt":
                    virtualkey = VirtualKeyCode.MENU;
                    break;
                case "ralt":
                    virtualkey = VirtualKeyCode.RMENU;
                    break;
                case "lcontrol":
                    virtualkey = VirtualKeyCode.CONTROL;
                    break;
                case "rcontrol":
                    virtualkey = VirtualKeyCode.RCONTROL;
                    break;
                case "lshift":
                    virtualkey = VirtualKeyCode.LSHIFT;
                    break;
                case "rshift":
                    virtualkey = VirtualKeyCode.RSHIFT;
                    break;
                case "lwin":
                    virtualkey = VirtualKeyCode.LWIN;
                    break;
                case "rwin":
                    virtualkey = VirtualKeyCode.RWIN;
                    break;
                case "delete":
                    virtualkey = VirtualKeyCode.DELETE;
                    break;
                case "tab":
                    virtualkey = VirtualKeyCode.TAB;
                    break;
                case "capslock":
                    virtualkey = VirtualKeyCode.CAPITAL;
                    break;
                case "pause":
                    virtualkey = VirtualKeyCode.PAUSE;
                    break;
                case "backspace":
                    virtualkey = VirtualKeyCode.BACK;
                    break;
                case "space":
                    virtualkey = VirtualKeyCode.SPACE;
                    break;
                case "pageup":
                    virtualkey = VirtualKeyCode.PRIOR;
                    break;
                case "pagedown":
                    virtualkey = VirtualKeyCode.NEXT;
                    break;
                case "end":
                    virtualkey = VirtualKeyCode.END;
                    break;
                case "home":
                    virtualkey = VirtualKeyCode.HOME;
                    break;
                case "left":
                    virtualkey = VirtualKeyCode.LEFT;
                    break;
                case "right":
                    virtualkey = VirtualKeyCode.RIGHT;
                    break;
                case "up":
                    virtualkey = VirtualKeyCode.UP;
                    break;
                case "down":
                    virtualkey = VirtualKeyCode.DOWN;
                    break;
                case "printscreen":
                    virtualkey = VirtualKeyCode.SNAPSHOT;
                    break;
                case "insert":
                    virtualkey = VirtualKeyCode.INSERT;
                    break;
                case "0":
                    virtualkey = VirtualKeyCode.VK_0;
                    break;
                case "1":
                    virtualkey = VirtualKeyCode.VK_1;
                    break;
                case "2":
                    virtualkey = VirtualKeyCode.VK_2;
                    break;
                case "3":
                    virtualkey = VirtualKeyCode.VK_3;
                    break;
                case "4":
                    virtualkey = VirtualKeyCode.VK_4;
                    break;
                case "5":
                    virtualkey = VirtualKeyCode.VK_5;
                    break;
                case "6":
                    virtualkey = VirtualKeyCode.VK_6;
                    break;
                case "7":
                    virtualkey = VirtualKeyCode.VK_7;
                    break;
                case "8":
                    virtualkey = VirtualKeyCode.VK_8;
                    break;
                case "9":
                    virtualkey = VirtualKeyCode.VK_9;
                    break;
                case "numlock":
                    virtualkey = VirtualKeyCode.NUMLOCK;
                    break;
                case "num0":
                    virtualkey = VirtualKeyCode.NUMPAD0;
                    break;
                case "num1":
                    virtualkey = VirtualKeyCode.NUMPAD1;
                    break;
                case "num2":
                    virtualkey = VirtualKeyCode.NUMPAD2;
                    break;
                case "num3":
                    virtualkey = VirtualKeyCode.NUMPAD3;
                    break;
                case "num4":
                    virtualkey = VirtualKeyCode.NUMPAD4;
                    break;
                case "num5":
                    virtualkey = VirtualKeyCode.NUMPAD5;
                    break;
                case "num6":
                    virtualkey = VirtualKeyCode.NUMPAD6;
                    break;
                case "num7":
                    virtualkey = VirtualKeyCode.NUMPAD7;
                    break;
                case "num8":
                    virtualkey = VirtualKeyCode.NUMPAD8;
                    break;
                case "num9":
                    virtualkey = VirtualKeyCode.NUMPAD9;
                    break;
                case "menu":
                    virtualkey = VirtualKeyCode.APPS;
                    break;
                case "f1":
                    virtualkey = VirtualKeyCode.F1;
                    break;
                case "f2":
                    virtualkey = VirtualKeyCode.F2;
                    break;
                case "f3":
                    virtualkey = VirtualKeyCode.F3;
                    break;
                case "f4":
                    virtualkey = VirtualKeyCode.F4;
                    break;
                case "f5":
                    virtualkey = VirtualKeyCode.F5;
                    break;
                case "f6":
                    virtualkey = VirtualKeyCode.F6;
                    break;
                case "f7":
                    virtualkey = VirtualKeyCode.F7;
                    break;
                case "f8":
                    virtualkey = VirtualKeyCode.F8;
                    break;
                case "f9":
                    virtualkey = VirtualKeyCode.F9;
                    break;
                case "f10":
                    virtualkey = VirtualKeyCode.F10;
                    break;
                case "f11":
                    virtualkey = VirtualKeyCode.F11;
                    break;
                case "f12":
                    virtualkey = VirtualKeyCode.F12;
                    break;
                case "mouse1":
                    virtualkey = VirtualKeyCode.F12;
                    break;
            }

            return virtualkey;
        }
    }
}
