//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;

namespace osu.Framework.Framework
{
    public abstract class BasicGameWindow
    {
        public event EventHandler ClientSizeChanged;
        public event EventHandler ScreenDeviceNameChanged;
        public event EventHandler Activated;
        public event EventHandler Deactivated;
        public event EventHandler Paint;

        public abstract Rectangle ClientBounds { get; }
        public abstract IntPtr Handle { get; }
        public abstract bool IsMinimized { get; }
        public abstract BasicGameForm Form { get; }

        public BasicGameWindow() { }

        public abstract void Close();

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value == null || title == value)
                    return;

                SetTitle(title = value);
            }
        }

        protected void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnClientSizeChanged()
        {
            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeactivated()
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPaint()
        {
            Paint?.Invoke(this, EventArgs.Empty);
        }

        protected void OnScreenDeviceNameChanged()
        {
            ScreenDeviceNameChanged?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void SetTitle(string title);
    }
}
