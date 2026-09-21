// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayBottomOrnament : OsuTestScene
    {
        private readonly TestOrnament ornament;

        public TestSceneRankedPlayBottomOrnament()
        {
            Child = new Container
            {
                Width = 400,
                Height = 24,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    ornament = new TestOrnament(),
                }
            };
        }

        [Test]
        public void TestAnimations()
        {
            AddStep("hide", () => ornament.Hide());
            AddStep("show", () => ornament.Show());
            AddSliderStep("Progress", 0f, 1f, 0f, p => ornament.Progress = p);
        }

        private partial class TestOrnament : RankedPlayBottomOrnament
        {
            public new float Progress
            {
                set => base.Progress = value;
            }
        }
    }
}
