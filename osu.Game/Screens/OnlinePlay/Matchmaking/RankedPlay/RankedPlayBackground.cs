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

        private BufferedContainer triangles1Buffered = null!;
        private Box bgBox = null!;
        private TrianglesV2 triangles2 = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                triangles1Buffered = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
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
                triangles2 = new TrianglesV2
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.4f, 1),
                    SpawnRatio = 2,
                    ClampAxes = Axes.Y,
                    RelativeSizeAxes = Axes.Both,
                },
            ];
        }

        public void FadeColours(Color4 top, Color4 bottom)
        {
            this.TransformTo(nameof(GradientTop), top, 1500, Easing.OutQuint);
            this.TransformTo(nameof(GradientBottom), bottom, 1500, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            bgBox.Colour = ColourInfo.GradientVertical(GradientTop, GradientBottom);
            triangles1Buffered.Colour = ColourInfo.GradientVertical(GradientTop.Lighten(0.2f), GradientBottom.Lighten(0.2f));
            triangles2.Colour = GradientTop.Lighten(0.5f);
        }
    }
}
