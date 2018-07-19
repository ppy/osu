// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContent : FillFlowContainer<ChangelogContentGroup>
    {
        public ChangelogContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding
            {
                Left = 70,
                Right = 70,
            };
        }
    }
}
