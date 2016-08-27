//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Windows.Forms;
using osu.Framework.Desktop.OS.Windows.Native;
using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS.Linux
{
    public class LinuxGameHost : BasicGameHost
    {
        public override BasicGameWindow Window => window;
        public override GLControl GLControl => window.Form;
        public override bool IsActive => true; // TODO LINUX

        private LinuxGameWindow window;

        internal LinuxGameHost(GraphicsContextFlags flags)
        {
            window = new LinuxGameWindow(flags);

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
            base.OnDeactivated(sender, args);
        }
    }
}
