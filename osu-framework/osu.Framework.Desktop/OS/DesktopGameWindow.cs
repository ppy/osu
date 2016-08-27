//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;
using osu.Framework.Desktop.OS.Windows;
using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS
{
    public abstract class DesktopGameWindow : BasicGameWindow
    {
        public override BasicGameForm Form { get; }

        public override Rectangle ClientBounds => Form.ClientBounds;
        public override bool IsMinimized => Form.IsMinimized;
        public override IntPtr Handle => Form.Handle;

        internal DesktopGameWindow(GraphicsContextFlags flags)
        {
            Form = CreateGameForm(flags);
            Form.ScreenChanged += delegate { OnScreenDeviceNameChanged(); };
            Form.ApplicationActivated += delegate { OnActivated(); };
            Form.ApplicationDeactivated += delegate { OnDeactivated(); };
            Form.SizeChanged += delegate { OnClientSizeChanged(); };
            Form.Closing += delegate { OnDeactivated(); };
            Form.Paint += delegate { OnPaint(); };
        }

        protected abstract BasicGameForm CreateGameForm(GraphicsContextFlags flags);

        public override void Close()
        {
            Form.Close();
        }

        protected override void SetTitle(string title)
        {
            Form.Text = title;
        }
    }
}
