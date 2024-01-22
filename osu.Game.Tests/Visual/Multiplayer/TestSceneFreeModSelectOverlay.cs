// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneFreeModSelectOverlay : MultiplayerTestScene
    {
        private FreeModSelectOverlay freeModSelectOverlay;
        private FooterButtonFreeMods footerButtonFreeMods;
        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGameBase)
        {
            availableMods.BindTo(osuGameBase.AvailableMods);
        }

        [Test]
        public void TestFreeModSelect()
        {
            createFreeModSelect();

            AddUntilStep("all visible mods are playable",
                () => this.ChildrenOfType<ModPanel>()
                          .Where(panel => panel.IsPresent)
                          .All(panel => panel.Mod.HasImplementation && panel.Mod.UserPlayable));

            AddToggleStep("toggle visibility", visible =>
            {
                if (freeModSelectOverlay != null)
                    freeModSelectOverlay.State.Value = visible ? Visibility.Visible : Visibility.Hidden;
            });
        }

        [Test]
        public void TestCustomisationNotAvailable()
        {
            createFreeModSelect();

            AddStep("select difficulty adjust", () => freeModSelectOverlay.SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            AddWaitStep("wait some", 3);
            AddAssert("customisation area not expanded", () => this.ChildrenOfType<ModSettingsArea>().Single().Height == 0);
        }

        [Test]
        public void TestSelectAllButtonUpdatesStateWhenSearchTermChanged()
        {
            createFreeModSelect();

            AddStep("apply search term", () => freeModSelectOverlay.SearchTerm = "ea");

            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("click select all button", navigateAndClick<SelectAllModsButton>);
            AddAssert("select all button disabled", () => !this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            AddStep("change search term", () => freeModSelectOverlay.SearchTerm = "e");

            AddAssert("select all button enabled", () => this.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);

            void navigateAndClick<T>() where T : Drawable
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<T>().Single());
                InputManager.Click(MouseButton.Left);
            }
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

            AddAssert("overlay select all button enabled", () => freeModSelectOverlay.ChildrenOfType<SelectAllModsButton>().Single().Enabled.Value);
            AddAssert("footer button displays off", () => footerButtonFreeMods.ChildrenOfType<IHasText>().Any(t => t.Text == "off"));

            AddStep("click footer select all button", () =>
            {
                InputManager.MoveMouseTo(footerButtonFreeMods);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("all mods selected", assertAllAvailableModsSelected);
            AddAssert("footer button displays all", () => footerButtonFreeMods.ChildrenOfType<IHasText>().Any(t => t.Text == "all"));

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !freeModSelectOverlay.SelectedMods.Value.Any());
            AddAssert("footer button displays off", () => footerButtonFreeMods.ChildrenOfType<IHasText>().Any(t => t.Text == "off"));
        }

        private void createFreeModSelect()
        {
            AddStep("create free mod select screen", () => Children = new Drawable[]
            {
                freeModSelectOverlay = new FreeModSelectOverlay
                {
                    State = { Value = Visibility.Visible }
                },
                footerButtonFreeMods = new FooterButtonFreeMods(freeModSelectOverlay)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Current = { BindTarget = freeModSelectOverlay.SelectedMods },
                },
            });
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
    }
}
