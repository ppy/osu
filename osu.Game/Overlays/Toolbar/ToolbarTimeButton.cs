// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarOverlayToggleButton
    {
        public string DateTimeString;

        public ToolbarTimeButton()
        {
            TooltipMain = "时间";
        }

        private DateTime GetTimeInfo()
        {
            var dt = DateTime.Now;
            return dt;
        }

        protected override void Update()
        {
            base.Update();

            DateTimeString = TooltipSub = GetTimeInfo().ToString() ?? "未知";
        }
        
        [BackgroundDependencyLoader(true)]
        private void load(TimeOverlay time)
        {
            StateContainer = time;
        }
    }
}
