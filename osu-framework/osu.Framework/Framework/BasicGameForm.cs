//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Framework
{
    public abstract class BasicGameForm : GLControl
    {
        public BasicGameForm(GraphicsContextFlags flags) : base(GraphicsMode.Default, 2, 0, flags)
        {
        }

        public event EventHandler ApplicationActivated;

        public event EventHandler ApplicationDeactivated;

        public abstract event EventHandler ScreenChanged;

        public event EventHandler UserResized;

        public abstract Rectangle ClientBounds { get; }

        public abstract bool IsMinimized { get; }

        public abstract void CentreToScreen();

        protected virtual void OnActivateApp(bool active)
        {
            if (active)
                ApplicationActivated?.Invoke(this, EventArgs.Empty);
            else
                ApplicationDeactivated?.Invoke(this, EventArgs.Empty);
        }
    }
}
