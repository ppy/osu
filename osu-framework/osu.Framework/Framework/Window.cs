//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;
using OpenTK;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using osu.Framework.Threading;

namespace osu.Framework.Framework
{
    public class Window
    {
        public delegate void ResolutionChangeSucceededEventHandler(bool spriteResultionChanged);

        private const int default_width = 1366;
        private const int default_height = 768;

        public event VoidDelegate OnSizeChanged;
        public event BoolDelegate OnMinimizedStateChanged;

        public BasicGameForm Form => host?.Window?.Form;
        public Size Size
        {
            get { return Form.ClientSize; }
            set { Form.ClientSize = value; }
        }

        public int Width => Size.Width;

        public int Height => Size.Height;

        public bool IsMinimized => Form.IsMinimized;

        public IntPtr Handle => gameWindow.Handle;

        private BasicGameHost host;
        private BasicGameWindow gameWindow => host?.Window;

        internal Window(BasicGameHost host)
        {
            this.host = host;

            Form.AllowDrop = true;
            Form.SizeChanged += Form_SizeChanged;

            Size = new Size(default_width, default_height);
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            OnSizeChanged?.Invoke();
        }

        public bool AllowDrop
        {
            get { return Form.AllowDrop; }
            set { Form.AllowDrop = value; }
        }

        public string Title
        {
            get { return gameWindow.Title; }
            set { gameWindow.Title = value; }
        }

        public void StealFocus()
        {
            Form.BringToFront();
            Form.Focus();
        }

        public void BringToFront()
        {
            Form.BringToFront();
            SetForegroundWindow(Form.Handle);
        }

        private NotifyIcon notifyIcon;
        private bool minimizedToTray;
        public bool MinimizedToTray
        {
            get { return minimizedToTray; }
            set
            {
                if (value == minimizedToTray)
                    return;
                minimizedToTray = value;

                if (minimizedToTray)
                {
                    if (notifyIcon == null)
                    {
                        notifyIcon = new NotifyIcon();
                        notifyIcon.Icon = Form.Icon;
                        notifyIcon.Click += (obj, e) => { MinimizedToTray = false; };
                    }

                    notifyIcon.Visible = true;

                    Form.WindowState = FormWindowState.Minimized;
                    Form.Visible = false;

                }
                else
                {
                    Form.Visible = true;
                    Form.WindowState = FormWindowState.Normal;

                    Form.ShowInTaskbar = true;
                    notifyIcon.Visible = false;

                    BringToFront();
                }

                OnMinimizedStateChanged?.Invoke(minimizedToTray);
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
    }
}
