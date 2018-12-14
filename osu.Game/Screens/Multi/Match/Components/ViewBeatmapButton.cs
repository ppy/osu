// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class ViewBeatmapButton : HeaderButton
    {
        public ViewBeatmapButton()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(200, 1);

            Text = "View beatmap";
        }
    }
}
