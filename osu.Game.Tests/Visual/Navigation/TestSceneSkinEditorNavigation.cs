// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;
using osuTK.Input;
using static osu.Game.Tests.Visual.Navigation.TestSceneScreenNavigation;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneSkinEditorNavigation : OsuGameTestScene
    {
        private TestPlaySongSelect songSelect;
        private SkinEditor skinEditor => Game.ChildrenOfType<SkinEditor>().FirstOrDefault();

        [Test]
        public void TestEditComponentDuringGameplay()
        {
            advanceToSongSelect();
            openSkinEditor();

            switchToGameplayScene();

            BarHitErrorMeter hitErrorMeter = null;

            AddUntilStep("select bar hit error blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().FirstOrDefault(b => b.Item is BarHitErrorMeter);

                if (blueprint == null)
                    return false;

                hitErrorMeter = (BarHitErrorMeter)blueprint.Item;
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
                return true;
            });

            AddAssert("value is default", () => hitErrorMeter.JudgementLineThickness.IsDefault);

            AddStep("hover first slider", () =>
            {
                InputManager.MoveMouseTo(
                    skinEditor.ChildrenOfType<SkinSettingsToolbox>().First()
                              .ChildrenOfType<SettingsSlider<float>>().First()
                              .ChildrenOfType<SliderBar<float>>().First()
                );
            });

            AddStep("adjust slider via keyboard", () => InputManager.Key(Key.Left));

            AddAssert("value is less than default", () => hitErrorMeter.JudgementLineThickness.Value < hitErrorMeter.JudgementLineThickness.Default);
        }

        [Test]
        public void TestComponentsDeselectedOnSkinEditorHide()
        {
            advanceToSongSelect();
            openSkinEditor();
            switchToGameplayScene();

            AddUntilStep("wait for components", () => skinEditor.ChildrenOfType<SkinBlueprint>().Any());

            AddStep("select all components", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.A);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("components selected", () => skinEditor.SelectedComponents.Count > 0);

            toggleSkinEditor();

            AddUntilStep("no components selected", () => skinEditor.SelectedComponents.Count == 0);
        }

        [Test]
        public void TestSwitchScreenWhileDraggingComponent()
        {
            Vector2 firstBlueprintCentre = Vector2.Zero;
            ScheduledDelegate movementDelegate = null;

            advanceToSongSelect();

            openSkinEditor();

            AddStep("add skinnable component", () =>
            {
                skinEditor.ChildrenOfType<SkinComponentToolbox.ToolboxComponentButton>().First().TriggerClick();
            });

            AddUntilStep("newly added component selected", () => skinEditor.SelectedComponents.Count == 1);

            AddStep("start drag", () =>
            {
                firstBlueprintCentre = skinEditor.ChildrenOfType<SkinBlueprint>().First().ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(firstBlueprintCentre);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("start movement", () => movementDelegate = Scheduler.AddDelayed(() => { InputManager.MoveMouseTo(firstBlueprintCentre += new Vector2(1)); }, 10, true));

            toggleSkinEditor();
            AddStep("exit song select", () => songSelect.Exit());

            AddUntilStep("wait for blueprints removed", () => !skinEditor.ChildrenOfType<SkinBlueprint>().Any());

            AddStep("stop drag", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
                movementDelegate?.Cancel();
            });
        }

        [Test]
        public void TestAutoplayCompatibleModsRetainedOnEnteringGameplay()
        {
            advanceToSongSelect();
            openSkinEditor();
            AddStep("select DT", () => Game.SelectedMods.Value = new Mod[] { new OsuModDoubleTime() });

            switchToGameplayScene();

            AddAssert("DT still selected", () => ((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Single() is OsuModDoubleTime);
        }

        [Test]
        public void TestAutoplayIncompatibleModsRemovedOnEnteringGameplay()
        {
            advanceToSongSelect();
            openSkinEditor();
            AddStep("select relax and spun out", () => Game.SelectedMods.Value = new Mod[] { new OsuModRelax(), new OsuModSpunOut() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        [Test]
        public void TestDuplicateAutoplayModRemovedOnEnteringGameplay()
        {
            advanceToSongSelect();
            openSkinEditor();
            AddStep("select autoplay", () => Game.SelectedMods.Value = new Mod[] { new OsuModAutoplay() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        [Test]
        public void TestCinemaModRemovedOnEnteringGameplay()
        {
            advanceToSongSelect();
            openSkinEditor();
            AddStep("select cinema", () => Game.SelectedMods.Value = new Mod[] { new OsuModCinema() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        [Test]
        public void TestModOverlayClosesOnOpeningSkinEditor()
        {
            advanceToSongSelect();
            AddStep("open mod overlay", () => songSelect.ModSelectOverlay.Show());

            openSkinEditor();
            AddUntilStep("mod overlay closed", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestChangeToNonSkinnableScreen()
        {
            advanceToSongSelect();
            openSkinEditor();
            AddAssert("blueprint container present", () => skinEditor.ChildrenOfType<SkinBlueprintContainer>().Count(), () => Is.EqualTo(1));
            AddAssert("placeholder not present", () => skinEditor.ChildrenOfType<NonSkinnableScreenPlaceholder>().Count(), () => Is.Zero);
            AddAssert("editor sidebars not empty", () => skinEditor.ChildrenOfType<EditorSidebar>().SelectMany(sidebar => sidebar.Children).Count(), () => Is.GreaterThan(0));

            AddStep("add skinnable component", () =>
            {
                skinEditor.ChildrenOfType<SkinComponentToolbox.ToolboxComponentButton>().First().TriggerClick();
            });
            AddUntilStep("newly added component selected", () => skinEditor.SelectedComponents, () => Has.Count.EqualTo(1));

            AddStep("exit to main menu", () => Game.ScreenStack.CurrentScreen.Exit());
            AddAssert("selection cleared", () => skinEditor.SelectedComponents, () => Has.Count.Zero);
            AddAssert("blueprint container not present", () => skinEditor.ChildrenOfType<SkinBlueprintContainer>().Count(), () => Is.Zero);
            AddAssert("placeholder present", () => skinEditor.ChildrenOfType<NonSkinnableScreenPlaceholder>().Count(), () => Is.EqualTo(1));
            AddAssert("editor sidebars empty", () => skinEditor.ChildrenOfType<EditorSidebar>().SelectMany(sidebar => sidebar.Children).Count(), () => Is.Zero);

            advanceToSongSelect();
            AddAssert("blueprint container present", () => skinEditor.ChildrenOfType<SkinBlueprintContainer>().Count(), () => Is.EqualTo(1));
            AddAssert("placeholder not present", () => skinEditor.ChildrenOfType<NonSkinnableScreenPlaceholder>().Count(), () => Is.Zero);
            AddAssert("editor sidebars not empty", () => skinEditor.ChildrenOfType<EditorSidebar>().SelectMany(sidebar => sidebar.Children).Count(), () => Is.GreaterThan(0));
        }

        private void advanceToSongSelect()
        {
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);
        }

        private void openSkinEditor()
        {
            toggleSkinEditor();
            AddUntilStep("skin editor loaded", () => skinEditor != null);
        }

        private void toggleSkinEditor()
        {
            AddStep("toggle skin editor", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.S);
                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
        }

        private void switchToGameplayScene()
        {
            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());
            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("Click gameplay scene button", () =>
            {
                InputManager.MoveMouseTo(skinEditor.ChildrenOfType<SkinEditorSceneLibrary.SceneButton>().First(b => b.Text.ToString() == "Gameplay"));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return Game.ScreenStack.CurrentScreen is Player;
            });
        }
    }
}
