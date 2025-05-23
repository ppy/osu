// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Screens.SelectV2;
using osuTK.Input;
using FooterButtonMods = osu.Game.Screens.SelectV2.FooterButtonMods;
using FooterButtonOptions = osu.Game.Screens.SelectV2.FooterButtonOptions;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelect : SongSelectTestScene
    {
        [Test]
        public void TestResultsScreenWhenClickingLeaderboardScore()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => Beatmaps.GetAllUsableBeatmapSets().Any(), () => Is.True);

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("import score", () =>
            {
                var beatmapInfo = Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First();
                ScoreManager.Import(new ScoreInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    BeatmapHash = beatmapInfo.Hash,
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new GuestUser(),
                });
            });

            AddStep("select ranking tab", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapDetailsArea.WedgeSelector<BeatmapDetailsArea.Header.Selection>>().Last());
                InputManager.Click(MouseButton.Left);
            });

            // probably should be done via dropdown menu instead of forcing this way?
            AddStep("set local scope", () =>
            {
                var current = LeaderboardManager.CurrentCriteria!;
                LeaderboardManager.FetchWithCriteria(current with
                {
                    Scope = BeatmapLeaderboardScope.Local,
                });
            });

            AddUntilStep("wait for score panel", () => SongSelect.ChildrenOfType<BeatmapLeaderboardScore>().Any());
            AddStep("click score panel", () =>
            {
                InputManager.MoveMouseTo(SongSelect.ChildrenOfType<BeatmapLeaderboardScore>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("wait for results screen", () => Stack.CurrentScreen is ResultsScreen);
        }

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

        [Test]
        public void TestClearModsViaModButtonRightClick()
        {
            LoadSongSelect();

            AddStep("select NC", () => SelectedMods.Value = new[] { new OsuModNightcore() });
            AddAssert("mods selected", () => SelectedMods.Value, () => Has.Count.EqualTo(1));
            AddStep("right click mod button", () =>
            {
                InputManager.MoveMouseTo(Footer.ChildrenOfType<FooterButtonMods>().Single());
                InputManager.Click(MouseButton.Right);
            });
            AddAssert("not mods selected", () => SelectedMods.Value, () => Has.Count.EqualTo(0));
        }

        [Test]
        public void TestSpeedChange()
        {
            LoadSongSelect();
            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());

            decreaseModSpeed();
            AddAssert("half time activated at 0.95x", () => SelectedMods.Value.OfType<ModHalfTime>().Single().SpeedChange.Value, () => Is.EqualTo(0.95).Within(0.005));

            decreaseModSpeed();
            AddAssert("half time speed changed to 0.9x", () => SelectedMods.Value.OfType<ModHalfTime>().Single().SpeedChange.Value, () => Is.EqualTo(0.9).Within(0.005));

            increaseModSpeed();
            AddAssert("half time speed changed to 0.95x", () => SelectedMods.Value.OfType<ModHalfTime>().Single().SpeedChange.Value, () => Is.EqualTo(0.95).Within(0.005));

            increaseModSpeed();
            AddAssert("no mods selected", () => SelectedMods.Value.Count == 0);

            increaseModSpeed();
            AddAssert("double time activated at 1.05x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().SpeedChange.Value, () => Is.EqualTo(1.05).Within(0.005));

            increaseModSpeed();
            AddAssert("double time speed changed to 1.1x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().SpeedChange.Value, () => Is.EqualTo(1.1).Within(0.005));

            decreaseModSpeed();
            AddAssert("double time speed changed to 1.05x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().SpeedChange.Value, () => Is.EqualTo(1.05).Within(0.005));

            OsuModNightcore nc = new OsuModNightcore
            {
                SpeedChange = { Value = 1.05 }
            };
            AddStep("select NC", () => SelectedMods.Value = new[] { nc });

            increaseModSpeed();
            AddAssert("nightcore speed changed to 1.1x", () => SelectedMods.Value.OfType<ModNightcore>().Single().SpeedChange.Value, () => Is.EqualTo(1.1).Within(0.005));

            decreaseModSpeed();
            AddAssert("nightcore speed changed to 1.05x", () => SelectedMods.Value.OfType<ModNightcore>().Single().SpeedChange.Value, () => Is.EqualTo(1.05).Within(0.005));

            decreaseModSpeed();
            AddAssert("no mods selected", () => SelectedMods.Value.Count == 0);

            decreaseModSpeed();
            AddAssert("daycore activated at 0.95x", () => SelectedMods.Value.OfType<ModDaycore>().Single().SpeedChange.Value, () => Is.EqualTo(0.95).Within(0.005));

            decreaseModSpeed();
            AddAssert("daycore activated at 0.95x", () => SelectedMods.Value.OfType<ModDaycore>().Single().SpeedChange.Value, () => Is.EqualTo(0.9).Within(0.005));

            increaseModSpeed();
            AddAssert("daycore activated at 0.95x", () => SelectedMods.Value.OfType<ModDaycore>().Single().SpeedChange.Value, () => Is.EqualTo(0.95).Within(0.005));

            OsuModDoubleTime dt = new OsuModDoubleTime
            {
                SpeedChange = { Value = 1.02 },
                AdjustPitch = { Value = true },
            };
            AddStep("select DT", () => SelectedMods.Value = new[] { dt });

            decreaseModSpeed();
            AddAssert("half time activated at 0.97x", () => SelectedMods.Value.OfType<ModHalfTime>().Single().SpeedChange.Value, () => Is.EqualTo(0.97).Within(0.005));
            AddAssert("adjust pitch preserved", () => SelectedMods.Value.OfType<ModHalfTime>().Single().AdjustPitch.Value, () => Is.True);

            OsuModHalfTime ht = new OsuModHalfTime
            {
                SpeedChange = { Value = 0.97 },
                AdjustPitch = { Value = true },
            };
            Mod[] modlist = { ht, new OsuModHardRock(), new OsuModHidden() };
            AddStep("select HT+HD", () => SelectedMods.Value = modlist);

            increaseModSpeed();
            AddAssert("double time activated at 1.02x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().SpeedChange.Value, () => Is.EqualTo(1.02).Within(0.005));
            AddAssert("double time activated at 1.02x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().AdjustPitch.Value, () => Is.True);
            AddAssert("HD still enabled", () => SelectedMods.Value.OfType<ModHidden>().SingleOrDefault(), () => Is.Not.Null);
            AddAssert("HR still enabled", () => SelectedMods.Value.OfType<ModHardRock>().SingleOrDefault(), () => Is.Not.Null);

            AddStep("select WU", () => SelectedMods.Value = new[] { new ModWindUp() });
            increaseModSpeed();
            AddAssert("windup still active", () => SelectedMods.Value.First() is ModWindUp);

            AddStep("select AS", () => SelectedMods.Value = new[] { new ModAdaptiveSpeed() });
            increaseModSpeed();
            AddAssert("adaptive speed still active", () => SelectedMods.Value.First() is ModAdaptiveSpeed);

            OsuModDoubleTime dtWithAdjustPitch = new OsuModDoubleTime
            {
                SpeedChange = { Value = 1.05 },
                AdjustPitch = { Value = true },
            };
            AddStep("select DT x1.05", () => SelectedMods.Value = new[] { dtWithAdjustPitch });

            decreaseModSpeed();
            AddAssert("no mods selected", () => SelectedMods.Value.Count == 0);

            decreaseModSpeed();
            AddAssert("half time activated at 0.95x", () => SelectedMods.Value.OfType<ModHalfTime>().Single().SpeedChange.Value, () => Is.EqualTo(0.95).Within(0.005));
            AddAssert("half time has adjust pitch active", () => SelectedMods.Value.OfType<ModHalfTime>().Single().AdjustPitch.Value, () => Is.True);

            AddStep("turn off adjust pitch", () => SelectedMods.Value.OfType<ModHalfTime>().Single().AdjustPitch.Value = false);

            increaseModSpeed();
            AddAssert("no mods selected", () => SelectedMods.Value.Count == 0);

            increaseModSpeed();
            AddAssert("double time activated at 1.05x", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().SpeedChange.Value, () => Is.EqualTo(1.05).Within(0.005));
            AddAssert("double time has adjust pitch inactive", () => SelectedMods.Value.OfType<ModDoubleTime>().Single().AdjustPitch.Value, () => Is.False);

            void increaseModSpeed() => AddStep("increase mod speed", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Up);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            void decreaseModSpeed() => AddStep("decrease mod speed", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Down);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
        }

        [Test]
        public void TestAutoplayShortcut()
        {
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddStep("press right", () => InputManager.Key(Key.Right)); // press right to select in carousel, also remove.
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("autoplay selected", () => SongSelect.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => SongSelect.IsCurrentScreen());

            AddAssert("no mods selected", () => SongSelect.Mods.Value.Count == 0);
        }

        [Test]
        public void TestAutoplayShortcutKeepsAutoplayIfSelectedAlready()
        {
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddStep("press right", () => InputManager.Key(Key.Right)); // press right to select in carousel, also remove.
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            ChangeMods(new OsuModAutoplay());

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("autoplay selected", () => SongSelect.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => SongSelect.IsCurrentScreen());

            AddAssert("autoplay still selected", () => SongSelect.Mods.Value.Single() is ModAutoplay);
        }

        [Test]
        public void TestAutoplayShortcutReturnsInitialModsOnExit()
        {
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddStep("press right", () => InputManager.Key(Key.Right)); // press right to select in carousel, also remove.
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            ChangeMods(new OsuModRelax());

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("only autoplay selected", () => SongSelect.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => SongSelect.IsCurrentScreen());

            AddAssert("relax returned", () => SongSelect.Mods.Value.Single() is ModRelax);
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
        public void TestFooterOptions()
        {
            LoadSongSelect();

            ImportBeatmapForRuleset(0);

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddAssert("options enabled", () => this.ChildrenOfType<FooterButtonOptions>().Single().Enabled.Value);

            AddStep("click", () => this.ChildrenOfType<FooterButtonOptions>().Single().TriggerClick());
            AddUntilStep("popover displayed", () => this.ChildrenOfType<FooterButtonOptions.Popover>().Any(p => p.IsPresent));
        }

        [Test]
        public void TestFooterOptionsState()
        {
            LoadSongSelect();

            ImportBeatmapForRuleset(0);

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().Single().Beatmaps.First()));

            AddAssert("options enabled", () => this.ChildrenOfType<FooterButtonOptions>().Single().Enabled.Value);
            AddStep("delete all beatmaps", () => Beatmaps.Delete());

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);
            AddStep("select no beatmap", () => Beatmap.SetDefault());

            AddUntilStep("wait for no beatmap", () => Beatmap.IsDefault);
            AddAssert("options disabled", () => !this.ChildrenOfType<FooterButtonOptions>().Single().Enabled.Value);
        }

        #endregion
    }
}
