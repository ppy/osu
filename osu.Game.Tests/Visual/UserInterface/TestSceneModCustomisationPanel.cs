// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModCustomisationPanel : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private ModCustomisationPanel panel = null!;
        private ModCustomisationHeader header = null!;
        private Container content = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = Array.Empty<Mod>();
            InputManager.MoveMouseTo(Vector2.One);

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(20f),
                Child = panel = new ModCustomisationPanel
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 400f,
                    State = { Value = Visibility.Visible },
                    SelectedMods = { BindTarget = SelectedMods },
                }
            };

            header = panel.Children.OfType<ModCustomisationHeader>().First();
            content = panel.Children.OfType<Container>().First();
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("set DT", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = true;
                panel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.ExpandedByMod;
            });
            AddStep("set DA", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModDifficultyAdjust() };
                panel.Enabled.Value = true;
                panel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.ExpandedByMod;
            });
            AddStep("set FL+WU+DA+AD", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModFlashlight(), new ModWindUp(), new OsuModDifficultyAdjust(), new OsuModApproachDifferent() };
                panel.Enabled.Value = true;
                panel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.ExpandedByMod;
            });
            AddStep("set empty", () =>
            {
                SelectedMods.Value = Array.Empty<Mod>();
                panel.Enabled.Value = false;
                panel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.Collapsed;
            });
        }

        [Test]
        public void TestHoverDoesNotExpandWhenNoCustomisableMods()
        {
            AddStep("hover header", () => InputManager.MoveMouseTo(header));

            checkExpanded(false);

            AddStep("hover content", () => InputManager.MoveMouseTo(content));

            checkExpanded(false);

            AddStep("left from content", () => InputManager.MoveMouseTo(Vector2.One));
        }

        [Test]
        public void TestHoverExpandsWithCustomisableMods()
        {
            AddStep("add customisable mod", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = true;
            });

            AddStep("hover header", () => InputManager.MoveMouseTo(header));
            checkExpanded(true);

            AddStep("move to content", () => InputManager.MoveMouseTo(content));
            checkExpanded(true);

            AddStep("move away", () => InputManager.MoveMouseTo(Vector2.One));
            checkExpanded(false);

            AddStep("hover header", () => InputManager.MoveMouseTo(header));
            checkExpanded(true);

            AddStep("move away", () => InputManager.MoveMouseTo(Vector2.One));
            checkExpanded(false);
        }

        [Test]
        public void TestHoverExpandsAndCollapsesWhenHeaderTouched()
        {
            AddStep("add customisable mod", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = true;
            });

            AddStep("touch header", () =>
            {
                var touch = new Touch(TouchSource.Touch1, header.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                Schedule(() => InputManager.EndTouch(touch));
            });
            checkExpanded(true);

            AddStep("touch away from header", () =>
            {
                var touch = new Touch(TouchSource.Touch1, header.ScreenSpaceDrawQuad.TopLeft - new Vector2(10));
                InputManager.BeginTouch(touch);
                Schedule(() => InputManager.EndTouch(touch));
            });
            checkExpanded(false);
        }

        [Test]
        public void TestDraggingKeepsPanelExpanded()
        {
            AddStep("add customisable mod", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = true;
            });

            AddStep("hover header", () => InputManager.MoveMouseTo(header));
            checkExpanded(true);

            AddStep("hover slider bar nub", () => InputManager.MoveMouseTo(panel.ChildrenOfType<OsuSliderBar<double>>().First().ChildrenOfType<Nub>().Single()));
            AddStep("hold", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag outside", () => InputManager.MoveMouseTo(Vector2.Zero));
            checkExpanded(true);

            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));
            checkExpanded(false);
        }

        private void checkExpanded(bool expanded)
        {
            AddUntilStep(expanded ? "is expanded" : "not expanded", () => panel.ExpandedState.Value,
                () => expanded ? Is.Not.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed) : Is.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed));
        }
    }
}
