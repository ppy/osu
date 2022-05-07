// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneFreeModSelectScreen : MultiplayerTestScene
    {
        private FreeModSelectScreen freeModSelectScreen;
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
                if (freeModSelectScreen != null)
                    freeModSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden;
            });
        }

        [Test]
        public void TestCustomisationNotAvailable()
        {
            createFreeModSelect();

            AddStep("select difficulty adjust", () => freeModSelectScreen.SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            AddWaitStep("wait some", 3);
            AddAssert("customisation area not expanded", () => this.ChildrenOfType<ModSettingsArea>().Single().Height == 0);
        }

        [Test]
        public void TestSelectDeselectAll()
        {
            createFreeModSelect();

            AddStep("click select all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ShearedButton>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods selected", assertAllAvailableModsSelected);

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ShearedButton>().Last());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !freeModSelectScreen.SelectedMods.Value.Any());
        }

        private void createFreeModSelect()
        {
            AddStep("create free mod select screen", () => Child = freeModSelectScreen = new FreeModSelectScreen
            {
                State = { Value = Visibility.Visible }
            });
            AddUntilStep("all column content loaded",
                () => freeModSelectScreen.ChildrenOfType<ModColumn>().Any()
                      && freeModSelectScreen.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded));
        }

        private bool assertAllAvailableModsSelected()
        {
            var allAvailableMods = availableMods.Value
                                                .SelectMany(pair => pair.Value)
                                                .Where(mod => mod.UserPlayable && mod.HasImplementation)
                                                .ToList();

            foreach (var availableMod in allAvailableMods)
            {
                if (freeModSelectScreen.SelectedMods.Value.All(selectedMod => selectedMod.GetType() != availableMod.GetType()))
                    return false;
            }

            return true;
        }
    }
}
