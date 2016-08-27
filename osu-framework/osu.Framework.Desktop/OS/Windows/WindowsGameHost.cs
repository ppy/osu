//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using osu.Framework.Desktop.OS.Windows.Native;
using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS.Windows
{
    public class WindowsGameHost : BasicGameHost
    {
        public override BasicGameWindow Window => window;
        public override GLControl GLControl => window.Form;
        public override bool IsActive => Window != null && GetForegroundWindow().Equals(Window.Handle);

        private WindowsGameWindow window;

        internal WindowsGameHost(GraphicsContextFlags flags)
        {
            Architecture.SetIncludePath();
            window = new WindowsGameWindow(flags);

            Application.EnableVisualStyles();

            Window.Activated += OnActivated;
            Window.Deactivated += OnDeactivated;
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous | Execution.ExecutionState.SystemRequired | Execution.ExecutionState.DisplayRequired);
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous);
            base.OnDeactivated(sender, args);
        }

        protected override void OnApplicationIdle(object sender, EventArgs e)
        {
            MSG message;
            while (!PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
                base.OnApplicationIdle(sender, e);
        }

        public override void Run()
        {
            OpenTK.NativeWindow.OsuWindowHandle = window.Handle;

            base.Run();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PeekMessage(out MSG msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax,
            uint flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hWnd;
            public WindowMessage msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        public enum WindowMessage : uint
        {
            ActivateApplication = 0x1c,
            Character = 0x102,
            Close = 0x10,
            Destroy = 2,
            EnterMenuLoop = 0x211,
            EnterSizeMove = 0x231,
            ExitMenuLoop = 530,
            ExitSizeMove = 0x232,
            GetMinMax = 0x24,
            KeyDown = 0x100,
            KeyUp = 0x101,
            LeftButtonDoubleClick = 0x203,
            LeftButtonDown = 0x201,
            LeftButtonUp = 0x202,
            MiddleButtonDoubleClick = 0x209,
            MiddleButtonDown = 0x207,
            MiddleButtonUp = 520,
            MouseFirst = 0x201,
            MouseLast = 0x20d,
            MouseMove = 0x200,
            MouseWheel = 0x20a,
            NonClientHitTest = 0x84,
            Paint = 15,
            PowerBroadcast = 0x218,
            Quit = 0x12,
            RightButtonDoubleClick = 0x206,
            RightButtonDown = 0x204,
            RightButtonUp = 0x205,
            SetCursor = 0x20,
            Size = 5,
            SystemCharacter = 0x106,
            SystemCommand = 0x112,
            SystemKeyDown = 260,
            SystemKeyUp = 0x105,
            XButtonDoubleClick = 0x20d,
            XButtonDown = 0x20b,
            XButtonUp = 0x20c
        }
    }
}
