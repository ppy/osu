// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneFreeModSelectOverlay : ScreenTestScene
    {
        private TestFreeModSelectOverlayScreen screen = null!;
        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();
        private readonly Bindable<IReadOnlyList<Mod>> freeMods = new Bindable<IReadOnlyList<Mod>>([]);

        private FreeModSelectOverlay freeModSelectOverlay => screen.Overlay;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGameBase)
        {
            availableMods.BindTo(osuGameBase.AvailableMods);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset selected mods", () => freeMods.Value = []);
        }

        [Test]
        public void TestFreeModSelect()
        {
            createFreeModSelect();

            AddUntilStep("all visible mods are playable",
                () => this.ChildrenOfType<ModPanel>()
                          .Where(panel => panel.IsPresent)
                          .All(panel => panel.Mod.HasImplementation && panel.Mod.UserPlayable));
        }

        [Test]
        public void TestCustomisationNotAvailable()
        {
            createFreeModSelect();

            AddStep("select difficulty adjust", () => freeModSelectOverlay.SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            AddWaitStep("wait some", 3);
            AddAssert("customisation area not expanded",
                () => this.ChildrenOfType<ModCustomisationPanel>().Single().ExpandedState.Value,
                () => Is.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed));
        }

        [Test]
        public void TestSelectAllButtonUpdatesStateWhenSearchTermChanged()
        {
            createFreeModSelect();

            AddStep("apply search term", () => freeModSelectOverlay.SearchTerm = "ea");

            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("click select all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("select all button disabled", () => !this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("change search term", () => freeModSelectOverlay.SearchTerm = "e");

            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestSelectDeselectAllViaKeyboard()
        {
            createFreeModSelect();

            AddStep("kill search bar focus", () => freeModSelectOverlay.SearchTextBox.KillFocus());

            AddStep("press ctrl+a", () => InputManager.Keys(PlatformAction.SelectAll));
            AddUntilStep("all mods selected", assertAllAvailableModsSelected);

            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddUntilStep("all mods deselected", () => !freeModSelectOverlay.SelectedMods.Value.Any());
        }

        [Test]
        public void TestSelectDeselectAll()
        {
            createFreeModSelect();

            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("click select all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods selected", assertAllAvailableModsSelected);
            AddAssert("select all button disabled", () => !this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !freeModSelectOverlay.SelectedMods.Value.Any());
            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestSelectAllViaFooterButtonThenDeselectFromOverlay()
        {
            createFreeModSelect();

            AddAssert("overlay select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);
            AddUntilStep(
                "footer button displays no mods",
                () => screen.Button.ChildrenOfType<InputBlockingContainer>().Single().IsPresent,
                () => Is.False
            );

            AddStep("click footer select all button", () =>
            {
                InputManager.MoveMouseTo(ScreenFooter.ChildrenOfType<SelectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("all mods selected", assertAllAvailableModsSelected);
            AddUntilStep(
                "footer button displays correct mod count",
                () => screen.Button.ChildrenOfType<FooterButtonMods.ModCountText>().Single().ChildrenOfType<IHasText>().Single().Text.ToString(),
                () => Is.EqualTo($"{freeMods.Value.Count} MODS")
            );

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !freeModSelectOverlay.SelectedMods.Value.Any());
            AddUntilStep(
                "footer button displays no mods",
                () => screen.Button.ChildrenOfType<InputBlockingContainer>().Single().IsPresent,
                () => Is.False
            );
        }

        private void createFreeModSelect()
        {
            AddStep("create free mod select screen", () => LoadScreen(screen = new TestFreeModSelectOverlayScreen
            {
                FreeMods = { BindTarget = freeMods },
            }));
            AddUntilStep("wait until screen is loaded", () => screen.IsLoaded, () => Is.True);
            AddStep("show overlay", () => freeModSelectOverlay.Show());
            AddUntilStep("all column content loaded",
                () => freeModSelectOverlay.ChildrenOfType<ModColumn>().Any()
                      && freeModSelectOverlay.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded));
        }

        private bool assertAllAvailableModsSelected()
        {
            var allAvailableMods = availableMods.Value
                                                .Where(pair => pair.Key != ModType.System)
                                                .SelectMany(pair => ModUtils.FlattenMods(pair.Value))
                                                .Where(mod => mod.UserPlayable && mod.HasImplementation)
                                                .ToList();

            if (freeModSelectOverlay.SelectedMods.Value.Count != allAvailableMods.Count)
                return false;

            foreach (var availableMod in allAvailableMods)
            {
                if (freeModSelectOverlay.SelectedMods.Value.All(selectedMod => selectedMod.GetType() != availableMod.GetType()))
                    return false;
            }

            return true;
        }

        private partial class TestFreeModSelectOverlayScreen : OsuScreen
        {
            public override bool ShowFooter => true;

            public FreeModSelectOverlay Overlay = null!;
            private IDisposable? overlayRegistration;

            public FooterButtonFreeModsV2 Button = null!;

            public readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>([]);

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

            [Cached]
            private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay = new FreeModSelectOverlay
                {
                    SelectedMods = { BindTarget = FreeMods }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                overlayRegistration = overlayManager?.RegisterBlockingOverlay(Overlay);
            }

            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() =>
            [
                Button = new FooterButtonFreeModsV2(Overlay)
                {
                    FreeMods = { BindTarget = FreeMods },
                },
            ];

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
            }
        }
    }
}
