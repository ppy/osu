// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneRoundedButton : ThemeComparisonTestScene
    {
        private readonly BindableBool enabled = new BindableBool(true);

        protected override Drawable CreateContent() => new RoundedButton
        {
            Width = 400,
            Text = "Test button",
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Enabled = { BindTarget = enabled },
        };

        [Test]
        public void TestDisabled()
        {
            AddToggleStep("toggle disabled", disabled => enabled.Value = !disabled);
        }

        [Test]
        public void TestBackgroundColour()
        {
            AddStep("set red scheme", () => CreateThemedContent(OverlayColourScheme.Red));
            AddAssert("first button has correct colour", () => Cell(0, 1).ChildrenOfType<RoundedButton>().First().BackgroundColour == new OverlayColourProvider(OverlayColourScheme.Red).Highlight1);
        }
    }
}
