// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModPanel : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestVariousPanels()
        {
            AddStep("create content", () => Child = new FillFlowContainer
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(0, 5),
                Children = new[]
                {
                    new ModPanel(new OsuModHalfTime()),
                    new ModPanel(new OsuModFlashlight()),
                    new ModPanel(new OsuModAutoplay()),
                    new ModPanel(new OsuModAlternate()),
                    new ModPanel(new OsuModApproachDifferent())
                }
            });
        }

        [Test]
        public void TestIncompatibilityDisplay()
        {
            IncompatibilityDisplayingModPanel panel = null;

            AddStep("create panel with DT", () =>
            {
                Child = panel = new IncompatibilityDisplayingModPanel(new OsuModDoubleTime())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = 300,
                };

                panel.Active.BindValueChanged(active =>
                {
                    SelectedMods.Value = active.NewValue
                        ? Array.Empty<Mod>()
                        : new[] { panel.Mod };
                });
            });

            clickPanel();
            AddAssert("panel active", () => panel.Active.Value);

            clickPanel();
            AddAssert("panel not active", () => !panel.Active.Value);

            AddStep("set incompatible mod", () => SelectedMods.Value = new[] { new OsuModHalfTime() });

            clickPanel();
            AddAssert("panel active", () => panel.Active.Value);

            void clickPanel() => AddStep("click panel", () =>
            {
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
