// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneTrianglesV2Background : OsuTestScene
    {
        private readonly TrianglesV2 triangles;
        private readonly Box box;

        public TestSceneTrianglesV2Background()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
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
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        },
                        new Container
                        {
                            Size = new Vector2(500, 100),
                            Masking = true,
                            CornerRadius = 40,
                            Child = box = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Spawn ratio", 0f, 10f, 1f, s =>
            {
                triangles.SpawnRatio = s;
                triangles.Reset(1234);
            });
            AddSliderStep("Thickness", 0f, 1f, 0.02f, t => triangles.Thickness = t);

            AddStep("White colour", () => box.Colour = triangles.Colour = Color4.White);
            AddStep("Vertical gradient", () => box.Colour = triangles.Colour = ColourInfo.GradientVertical(Color4.White, Color4.Red));
            AddStep("Horizontal gradient", () => box.Colour = triangles.Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.Red));
        }
    }
}
