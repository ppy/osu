// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarButton
    {

        public string DateTimeString;

        public ToolbarTimeButton()
        {
            SetIcon(FontAwesome.Solid.Clock);
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
    }
}
