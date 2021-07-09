// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Sections;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneProfileSubsectionHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private ProfileSubsectionHeader header;

        [Test]
        public void TestHiddenCounter()
        {
            AddStep("Create header", () => createHeader("Header with hidden counter", CounterVisibilityState.AlwaysHidden));
            AddAssert("Value is 0", () => header.Current.Value == 0);
            AddAssert("Counter is hidden", () => header.ChildrenOfType<CounterPill>().First().Alpha == 0);
            AddStep("Set count 10", () => header.Current.Value = 10);
            AddAssert("Value is 10", () => header.Current.Value == 10);
            AddAssert("Counter is hidden", () => header.ChildrenOfType<CounterPill>().First().Alpha == 0);
        }

        [Test]
        public void TestVisibleCounter()
        {
            AddStep("Create header", () => createHeader("Header with visible counter", CounterVisibilityState.AlwaysVisible));
            AddAssert("Value is 0", () => header.Current.Value == 0);
            AddAssert("Counter is visible", () => header.ChildrenOfType<CounterPill>().First().Alpha == 1);
            AddStep("Set count 10", () => header.Current.Value = 10);
            AddAssert("Value is 10", () => header.Current.Value == 10);
            AddAssert("Counter is visible", () => header.ChildrenOfType<CounterPill>().First().Alpha == 1);
        }

        [Test]
        public void TestVisibleWhenZeroCounter()
        {
            AddStep("Create header", () => createHeader("Header with visible when zero counter", CounterVisibilityState.VisibleWhenZero));
            AddAssert("Value is 0", () => header.Current.Value == 0);
            AddAssert("Counter is visible", () => header.ChildrenOfType<CounterPill>().First().Alpha == 1);
            AddStep("Set count 10", () => header.Current.Value = 10);
            AddAssert("Value is 10", () => header.Current.Value == 10);
            AddAssert("Counter is hidden", () => header.ChildrenOfType<CounterPill>().First().Alpha == 0);
            AddStep("Set count 0", () => header.Current.Value = 0);
            AddAssert("Value is 0", () => header.Current.Value == 0);
            AddAssert("Counter is visible", () => header.ChildrenOfType<CounterPill>().First().Alpha == 1);
        }

        [Test]
        public void TestInitialVisibility()
        {
            AddStep("Create header with 0 value", () => createHeader("Header with visible when zero counter", CounterVisibilityState.VisibleWhenZero));
            AddAssert("Value is 0", () => header.Current.Value == 0);
            AddAssert("Counter is visible", () => header.ChildrenOfType<CounterPill>().First().Alpha == 1);

            AddStep("Create header with 1 value", () => createHeader("Header with visible when zero counter", CounterVisibilityState.VisibleWhenZero, 1));
            AddAssert("Value is 1", () => header.Current.Value == 1);
            AddAssert("Counter is hidden", () => header.ChildrenOfType<CounterPill>().First().Alpha == 0);
        }

        private void createHeader(string text, CounterVisibilityState state, int initialValue = 0)
        {
            Clear();
            Add(header = new ProfileSubsectionHeader(text, state)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { Value = initialValue }
            });
        }
    }
}
