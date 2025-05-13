// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Select;
using osuTK.Input;
using FooterButtonMods = osu.Game.Screens.SelectV2.FooterButtonMods;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelect : SongSelectTestScene
    {
        #region Hotkeys

        [Test]
        public void TestDeleteHotkey()
        {
            LoadSongSelect();

            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("press shift-delete", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Delete);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            AddUntilStep("delete dialog shown", () => DialogOverlay.CurrentDialog, Is.InstanceOf<BeatmapDeleteDialog>);
            AddStep("confirm deletion", () => DialogOverlay.CurrentDialog!.PerformAction<PopupDialogDangerousButton>());

            AddAssert("beatmap set deleted", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.False);
        }

        #endregion

        #region Footer

        [Test]
        public void TestFooterMods()
        {
            LoadSongSelect();

            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });
            AddStep("two mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock() });
            AddStep("three mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime() });
            AddStep("four mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic() });
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });

            AddStep("modified", () => SelectedMods.Value = new List<Mod> { new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + one", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + two", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + three",
                () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + four",
                () => SelectedMods.Value = new List<Mod>
                    { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDifficultyAdjust(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });
        }

        [Test]
        public void TestFooterModOverlay()
        {
            LoadSongSelect();

            AddStep("Press F1", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<FooterButtonMods>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Overlay visible", () => this.ChildrenOfType<ModSelectOverlay>().Single().State.Value == Visibility.Visible);
            AddStep("Hide", () => this.ChildrenOfType<ModSelectOverlay>().Single().Hide());
        }

        // add these test cases when functionality is implemented.
        // [Test]
        // public void TestFooterRandom()
        // {
        //     loadSongSelect();
        //
        //     AddStep("press F2", () => InputManager.Key(Key.F2));
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRandomViaMouse()
        // {
        //     loadSongSelect();
        //
        //     AddStep("click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //     });
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewind()
        // {
        //     loadSongSelect();
        //
        //     AddStep("press Shift+F2", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.PressKey(Key.F2);
        //         InputManager.ReleaseKey(Key.F2);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaShiftMouseLeft()
        // {
        //     loadSongSelect();
        //
        //     AddStep("shift + click button", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaMouseRight()
        // {
        //     loadSongSelect();
        //
        //     AddStep("right click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Right);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }

        [Test]
        public void TestFooterShowOptions()
        {
            LoadSongSelect();

            AddStep("enable options", () =>
            {
                var optionsButton = this.ChildrenOfType<ScreenFooterButton>().Last();

                optionsButton.Enabled.Value = true;
                optionsButton.TriggerClick();
            });
        }

        [Test]
        public void TestFooterOptionsState()
        {
            LoadSongSelect();

            AddToggleStep("set options enabled state", state => this.ChildrenOfType<ScreenFooterButton>().Last().Enabled.Value = state);
        }

        #endregion
    }
}
