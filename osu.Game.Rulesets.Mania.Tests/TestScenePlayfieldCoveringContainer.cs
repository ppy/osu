// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestScenePlayfieldCoveringContainer : OsuTestScene
    {
        private readonly ScrollingTestContainer scrollingContainer;
        private readonly PlayfieldCoveringWrapper cover;

        public TestScenePlayfieldCoveringContainer()
        {
            Child = scrollingContainer = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 500),
                Child = cover = new PlayfieldCoveringWrapper(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Orange
                })
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [Test]
        public void TestScrollingDownwards()
        {
            AddStep("set down scroll", () => scrollingContainer.Direction = ScrollingDirection.Down);
            AddStep("set coverage = 0.5", () => cover.Coverage.Value = 0.5f);
            AddStep("set coverage = 0.8f", () => cover.Coverage.Value = 0.8f);
            AddStep("set coverage = 0.2f", () => cover.Coverage.Value = 0.2f);
        }

        [Test]
        public void TestScrollingUpwards()
        {
            AddStep("set up scroll", () => scrollingContainer.Direction = ScrollingDirection.Up);
            AddStep("set coverage = 0.5", () => cover.Coverage.Value = 0.5f);
            AddStep("set coverage = 0.8f", () => cover.Coverage.Value = 0.8f);
            AddStep("set coverage = 0.2f", () => cover.Coverage.Value = 0.2f);
        }
    }
}
