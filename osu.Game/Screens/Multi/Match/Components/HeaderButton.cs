// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class HeaderButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = OsuColour.FromHex(@"1187aa");

            Triangles.ColourLight = OsuColour.FromHex(@"277b9c");
            Triangles.ColourDark = OsuColour.FromHex(@"1f6682");
            Triangles.TriangleScale = 1.5f;

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 1f,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.15f,
                    Blending = BlendingParameters.Additive,
                },
            });
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.GetFont(weight: FontWeight.Light, size: 30),
        };
    }
}
