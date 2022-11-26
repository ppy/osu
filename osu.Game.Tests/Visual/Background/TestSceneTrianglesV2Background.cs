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
    public partial class TestSceneTrianglesV2Background : OsuTestScene
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
                    Size = new Vector2(500, 100),
                    Masking = true,
                    CornerRadius = 40,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red
                        },
                        triangles = new TrianglesV2
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ColourTop = Color4.White,
                            ColourBottom = Color4.Red
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Spawn ratio", 0f, 2f, 1f, s => triangles.SpawnRatio = s);
            AddSliderStep("Thickness", 0f, 1f, 0.02f, t => triangles.Thickness = t);
        }
    }
}
