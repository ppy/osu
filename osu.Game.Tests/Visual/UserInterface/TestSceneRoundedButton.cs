// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneRoundedButton : ThemeComparisonTestScene
    {
        private readonly BindableBool enabled = new BindableBool(true);

        protected override Drawable CreateContent()
        {
            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new RoundedButton
                    {
                        Width = 400,
                        Text = "Test button",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Enabled = { BindTarget = enabled },
                    },
                    new SettingsButton
                    {
                        Text = "Test settings button",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Enabled = { BindTarget = enabled },
                    },
                }
            };
        }

        [Test]
        public void TestDisabled()
        {
            AddToggleStep("toggle disabled", disabled => enabled.Value = !disabled);
        }

        [Test]
        public void TestBackgroundColour()
        {
            AddStep("set red scheme", () => CreateThemedContent(OverlayColourScheme.Red));
            AddAssert("rounded button has correct colour", () => Cell(0, 1).ChildrenOfType<RoundedButton>().First().BackgroundColour == new OverlayColourProvider(OverlayColourScheme.Red).Colour3);
            AddAssert("settings button has correct colour", () => Cell(0, 1).ChildrenOfType<SettingsButton>().First().BackgroundColour == new OverlayColourProvider(OverlayColourScheme.Red).Colour3);
        }
    }
}
