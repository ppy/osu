// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;

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
                    ColourDark = Color4.Gray
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Triangle scale", 0f, 10f, 1f, s => triangles.TriangleScale = s);
        }
    }
}
