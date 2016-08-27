//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;
using System.Windows.Forms;
using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS.Linux
{
    public class LinuxGameForm : BasicGameForm
    {
        public override bool IsMinimized => ClientSize.Width != 0 || ClientSize.Height == 0;
        
        public override event EventHandler ScreenChanged;

        private Screen screen;

        internal LinuxGameForm(GraphicsContextFlags flags) : base(flags)
        {
            SuspendLayout();
            CausesValidation = false;
            ClientSize = new Size(1, 1);
            BackColor = Color.Black;
            ResumeLayout(false);

            LocationChanged += delegate { updateScreen(); };
            ClientSizeChanged += delegate { updateScreen(); };
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            //stop alt/f10 from freezing form rendering.
            if (((keyData & Keys.Alt) == Keys.Alt && (keyData & Keys.F4) != Keys.F4) || (keyData & Keys.F10) == Keys.F10)
                return true;
            return base.ProcessDialogKey(keyData);
        }

        public override Rectangle ClientBounds
        {
            get
            {
                Point point = PointToScreen(Point.Empty);
                return new Rectangle(point.X, point.Y, ClientSize.Width, ClientSize.Height);
            }
        }

        public override void CentreToScreen()
        {
            CenterToScreen();
        }
        
        private void updateScreen()
        {
            var screen = Screen.FromHandle(Handle);
            if ((this.screen == null) || !this.screen.Equals(screen))
            {
                this.screen = screen;
                if (this.screen != null)
                    ScreenChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
