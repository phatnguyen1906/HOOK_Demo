using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseHook_noUIAutomatic
{
    public class GlobalMouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        //
        private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private MouseHookHandler hookHandler;
        private IntPtr hookID = IntPtr.Zero;

        public event MouseEventHandler OnMouseDown;
        public event MouseEventHandler OnMouseUp;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookHandler lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public GlobalMouseHook()
        {
            hookHandler = HookCallback;
        }

        public void Start()
        {
            hookID = SetWindowsHookEx(WH_MOUSE_LL, hookHandler,
                GetModuleHandle(null), 0);
        }

        public void Stop()
        {
            if (hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookID);
                hookID = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    OnMouseDown?.Invoke(this, GetMouseEventArgs(lParam));
                }
                else if (wParam == (IntPtr)WM_LBUTTONUP)
                {
                    OnMouseUp?.Invoke(this, GetMouseEventArgs(lParam));
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private MouseEventArgs GetMouseEventArgs(IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(
                lParam, typeof(MSLLHOOKSTRUCT));
            return new MouseEventArgs(
                MouseButtons.Left,
                1,
                hookStruct.pt.x,
                hookStruct.pt.y,
                0
            );
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
