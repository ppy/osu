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
    public class TestSceneTrianglesBackground : OsuTestScene
    {
        public TestSceneTrianglesBackground()
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
                    Size = new Vector2(200),
                    Masking = true,
                    CornerRadius = 40,
                    Child = new TrianglesV2
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
    }
}
