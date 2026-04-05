// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayBackground : CompositeDrawable
    {
        public Color4 GradientBottom = Color4Extensions.FromHex("#15061e");
        public Color4 GradientTop = Color4Extensions.FromHex("#240d36");

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                new Box
                {
                    Colour = ColourInfo.GradientVertical(GradientTop, GradientBottom),
                    RelativeSizeAxes = Axes.Both,
                },
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(GradientTop.Lighten(0.2f), GradientBottom.Lighten(0.2f)),
                    Children = new Drawable[]
                    {
                        new Triangles
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(0.5f, 1),
                            SpawnRatio = 1.4f,
                            ClampAxes = Axes.Y,
                            Velocity = 0.5f,
                            TriangleScale = 4,
                            ColourLight = Color4.White,
                            ColourDark = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
                new TrianglesV2
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.4f, 1),
                    SpawnRatio = 2,
                    Colour = GradientTop.Lighten(0.5f),
                    ClampAxes = Axes.Y,
                    RelativeSizeAxes = Axes.Both,
                },
            ];
        }
    }
}
