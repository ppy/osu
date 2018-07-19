// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Changelog
{
    // maybe look to osu.Game.Screens.Play.SquareGraph for reference later
    public class ChangelogChart : BufferedContainer
    {
        public ChangelogChart()
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = StreamColour.STABLE,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteText
                {
                    Text = "Graph Placeholder",
                    TextSize = 28,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}
