// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo

namespace osu.Desktop.Windows
{
    internal static class WindowsKey
    {
        private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, UIntPtr wParam, ref KdDllHookStruct lParam);

        private static bool isBlocked;

        private const int wh_keyboard_ll = 13;
        private const int wm_keydown = 256;
        private const int wm_syskeyup = 261;

        //Resharper disable once NotAccessedField.Local
        private static LowLevelKeyboardProcDelegate? keyboardHookDelegate; // keeping a reference alive for the GC
        private static IntPtr keyHook;

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct KdDllHookStruct
        {
            [FieldOffset(0)]
            public readonly int VkCode;

            [FieldOffset(8)]
            public readonly int Flags;
        }

        private static IntPtr lowLevelKeyboardProc(int nCode, UIntPtr wParam, ref KdDllHookStruct lParam)
        {
            ulong message = wParam.ToUInt64();

            if (nCode >= 0 && message >= wm_keydown && message <= wm_syskeyup)
            {
                switch (lParam.VkCode)
                {
                    case 0x5B: // left windows key
                    case 0x5C: // right windows key
                        return 1;
                }
            }

            return callNextHookEx(IntPtr.Zero, nCode, wParam, ref lParam);
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

            unhookWindowsHookEx(keyHook);
            keyboardHookDelegate = null;

            keyHook = IntPtr.Zero;

            isBlocked = false;
        }

        [DllImport(@"user32.dll", EntryPoint = @"SetWindowsHookExA")]
        private static extern IntPtr setWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport(@"user32.dll", EntryPoint = @"UnhookWindowsHookEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool unhookWindowsHookEx(IntPtr hHook);

        [DllImport(@"user32.dll", EntryPoint = @"CallNextHookEx")]
        private static extern IntPtr callNextHookEx(IntPtr hHook, int nCode, UIntPtr wParam, ref KdDllHookStruct lParam);
    }
}
