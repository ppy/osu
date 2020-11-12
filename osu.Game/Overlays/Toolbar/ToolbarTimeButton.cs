// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarButton
    {
        public ToolbarTimeButton()
        {
            TooltipMain = "时间";
            TooltipSub = "现在几点了?";

            Width = 1;
            AutoSizeAxes = Axes.X;
        }

        protected override void Update()
        {
            base.Update();

            DrawableText.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        }
    }
}
