// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePracticeRangeControl : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestBasic()
        {
            AddStep("create Control", () => Child = new PracticeSegmentSliderComponent
            {
                Width = 200,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3),
            });
        }
    }
}
