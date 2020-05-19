// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Sections;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneProfileCounterPill : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private readonly CounterPill pill;
        private readonly BindableInt value = new BindableInt();

        public TestSceneProfileCounterPill()
        {
            Child = pill = new CounterPill
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { BindTarget = value }
            };
        }

        [Test]
        public void TestVisibility()
        {
            AddStep("Set value to 0", () => value.Value = 0);
            AddAssert("Check hidden", () => !pill.IsPresent);
            AddStep("Set value to 10", () => value.Value = 10);
            AddAssert("Check visible", () => pill.IsPresent);
        }
    }
}
