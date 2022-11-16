// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Tests.Visual.Background
{
    public class TestSceneTrianglesV2Background : OsuTestScene
    {
        private readonly TrianglesV2 triangles;

        public TestSceneTrianglesV2Background()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500),
                    Masking = true,
                    CornerRadius = 40,
                    Child = triangles = new TrianglesV2
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ColourTop = Color4.Red,
                        ColourBottom = Color4.Orange
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Spawn ratio", 0f, 2f, 1f, s => triangles.SpawnRatio = s);
        }
    }
}
