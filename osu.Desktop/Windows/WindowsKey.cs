// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo

namespace osu.Desktop.Windows
{
    internal class WindowsKey
    {
        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KdDllHookStruct lParam);

        private static bool isBlocked;

        private const int wh_keyboard_ll = 13;
        private const int wm_keydown = 256;
        private const int wm_syskeyup = 261;

        //Resharper disable once NotAccessedField.Local
        private static LowLevelKeyboardProcDelegate keyboardHookDelegate; // keeping a reference alive for the GC
        private static IntPtr keyHook;

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct KdDllHookStruct
        {
            [FieldOffset(0)]
            public readonly int VkCode;

            [FieldOffset(8)]
            public readonly int Flags;
        }

        private static int lowLevelKeyboardProc(int nCode, int wParam, ref KdDllHookStruct lParam)
        {
            if (wParam >= wm_keydown && wParam <= wm_syskeyup)
            {
                switch (lParam.VkCode)
                {
                    case 0x5B: // left windows key
                    case 0x5C: // right windows key
                        return 1;
                }
            }

            return callNextHookEx(0, nCode, wParam, ref lParam);
        }

        internal static void Disable()
        {
            if (keyHook != IntPtr.Zero || isBlocked)
                return;

            keyHook = setWindowsHookEx(wh_keyboard_ll, (keyboardHookDelegate = lowLevelKeyboardProc), Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);

            isBlocked = true;
        }

        internal static void Enable()
        {
            if (keyHook == IntPtr.Zero || !isBlocked)
                return;

            keyHook = unhookWindowsHookEx(keyHook);
            keyboardHookDelegate = null;

            keyHook = IntPtr.Zero;

            isBlocked = false;
        }

        [DllImport(@"user32.dll", EntryPoint = @"SetWindowsHookExA")]
        private static extern IntPtr setWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport(@"user32.dll", EntryPoint = @"UnhookWindowsHookEx")]
        private static extern IntPtr unhookWindowsHookEx(IntPtr hHook);

        [DllImport(@"user32.dll", EntryPoint = @"CallNextHookEx")]
        private static extern int callNextHookEx(int hHook, int nCode, int wParam, ref KdDllHookStruct lParam);
    }
}
