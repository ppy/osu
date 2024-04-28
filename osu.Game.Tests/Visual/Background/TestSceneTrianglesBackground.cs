// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneTrianglesBackground : OsuTestScene
    {
        private readonly Triangles triangles;

        public TestSceneTrianglesBackground()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                triangles = new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = Color4.White,
                    ColourDark = Color4.Gray,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.9f),
                    ClampAxes = Axes.None
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Triangle scale", 0f, 10f, 1f, s => triangles.TriangleScale = s);
            AddSliderStep("Seed", 0, 1000, 0, s => triangles.Reset(s));
            AddStep("ClampAxes X", () => triangles.ClampAxes = Axes.X);
            AddStep("ClampAxes Y", () => triangles.ClampAxes = Axes.Y);
            AddStep("ClampAxes Both", () => triangles.ClampAxes = Axes.Both);
            AddStep("ClampAxes None", () => triangles.ClampAxes = Axes.None);
        }
    }
}
