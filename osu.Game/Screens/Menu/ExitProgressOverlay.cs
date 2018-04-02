using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Screens.Menu
{
    public class ExitProgressOverlay : OverlayContainer
    {
        public double Progress { get; set; }
        public override bool HandleKeyboardInput => false;
        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;
    }
}
