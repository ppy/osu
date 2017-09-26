// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.BeatmapSet
{
    public class HeaderButton : OsuClickableContainer
    {
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public HeaderButton()
        {
            CornerRadius = 3;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"094c5f"),
                },
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = OsuColour.FromHex(@"0f7c9b"),
                    ColourDark = OsuColour.FromHex(@"094c5f"),
                    TriangleScale = 1.5f,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
