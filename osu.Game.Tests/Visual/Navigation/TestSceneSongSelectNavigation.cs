// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    /// <summary>
    /// Tests copied out of `TestSceneScreenNavigation` which are specific to song select.
    /// These are for SongSelectV2. Eventually, the tests in the above class should be deleted along with old song select.
    /// </summary>
    public partial class TestSceneSongSelectNavigation : OsuGameTestScene
    {
        [Test]
        public void TestRetryFromResults()
        {
            var getOriginalPlayer = playToResults();

            AddStep("attempt to retry", () => ((ResultsScreen)Game.ScreenStack.CurrentScreen).ChildrenOfType<HotkeyRetryOverlay>().First().Action());
            AddUntilStep("wait for player", () => Game.ScreenStack.CurrentScreen != getOriginalPlayer() && Game.ScreenStack.CurrentScreen is Player);
        }

        [Test]
        public void TestPushSongSelectAndClickBottomLeftCorner()
        {
            AddStep("push song select", () => Game.ScreenStack.Push(new SoloSongSelect()));

            // TODO: without this step, a critical bug will be hit, see inline comment in `OsuGame.handleBackButton`.
            AddUntilStep("Wait for song select", () => Game.ScreenStack.CurrentScreen is SoloSongSelect select && select.IsLoaded);

            AddStep("click in corner", () =>
            {
                InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomLeft);
                InputManager.Click(MouseButton.Left);
            });

            ConfirmAtMainMenu();
        }

        [Test]
        public void TestPushSongSelectAndPressBackButtonImmediately()
        {
            AddStep("push song select", () => Game.ScreenStack.Push(new SoloSongSelect()));

            // TODO: without this step, a critical bug will be hit, see inline comment in `OsuGame.handleBackButton`.
            AddUntilStep("Wait for song select", () => Game.ScreenStack.CurrentScreen is SoloSongSelect select && select.IsLoaded);

            AddStep("press back button", () => Game.ChildrenOfType<ScreenBackButton>().First().Action!.Invoke());

            ConfirmAtMainMenu();
        }

        [Test]
        public void TestEditBeatmap()
        {
            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("open menu", () => InputManager.Key(Key.F3));
            AddStep("trigger edit", () =>
            {
                // TODO: should be 5, not 4.
                InputManager.Key(Key.Number4);
            });

            waitForScreen<Editor>();

            pushEscape();
            waitForScreen<SoloSongSelect>();
        }

        [Test]
        public void TestPresentBeatmapFromMainMenuUsesPreviewPoint()
        {
            BeatmapSetInfo beatmapInfo = null!;

            AddStep("import beatmap", () =>
            {
                var task = BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true);
                task.WaitSafely();
                beatmapInfo = task.GetResultSafely();
            });

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapInfo));

            AddUntilStep("wait for track playing", () => Game.MusicController.IsPlaying);

            AddAssert("ensure time is reset to preview point",
                () =>
                {
                    double timeFromPreviewPoint = Math.Abs(Game.MusicController.CurrentTrack.CurrentTime - beatmapInfo.Metadata.PreviewTime);
                    return timeFromPreviewPoint < 5000;
                });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player? player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            if (withUserPause)
                AddStep("pause", () => Game.Dependencies.Get<MusicController>().Stop(true));

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for fail", () => player?.GameplayState.HasFailed, () => Is.True);

            AddUntilStep("wait for track stop", () => !Game.MusicController.IsPlaying);
            AddAssert("Ensure time before preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => Game.MusicController.IsPlaying);
            AddAssert("Ensure time wasn't reset to preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);
        }

        [Test]
        public void TestAutoplayShortcutReturnsInitialModsOnExit()
        {
            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selection", () => !Game.Beatmap.IsDefault);

            AddStep("open mod select", () => InputManager.Key(Key.F1));
            AddStep("search magnetised", () => this.ChildrenOfType<ModSelectOverlay>().Single().SearchTerm = "MG");
            AddStep("select", () => InputManager.Key(Key.Enter));

            AddAssert("magnetised selected", () => Game.SelectedMods.Value.Single(), Is.TypeOf<OsuModMagnetised>);
            AddStep("configure mod", () => ((OsuModMagnetised)Game.SelectedMods.Value.Single()).AttractionStrength.Value = 1.0f);

            pushEscape();
            pushEscape();

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return Game.ScreenStack.CurrentScreen is Player;
            });

            AddAssert("only autoplay selected", () => Game.SelectedMods.Value.Single(), Is.TypeOf<OsuModAutoplay>);

            pushEscape();
            waitForScreen<SoloSongSelect>();

            AddAssert("magnetised selected", () => Game.SelectedMods.Value.Single(), Is.TypeOf<OsuModMagnetised>);
            AddAssert("mod configured", () => ((OsuModMagnetised)Game.SelectedMods.Value.Single()).AttractionStrength.Value, () => Is.EqualTo(1.0f));
        }

        [Test]
        public void TestLeaderboardCorrectInPlayer()
        {
            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);
            AddStep("switch to next difficulty and immediately press enter", () =>
            {
                InputManager.Key(Key.Down);
                Schedule(() => InputManager.Key(Key.Enter));
            });

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return Game.ScreenStack.CurrentScreen is Player;
            });
            AddAssert("leaderboard matches gameplay beatmap", () => Game.ChildrenOfType<LeaderboardManager>().Single().CurrentCriteria?.Beatmap, () => Is.EqualTo(beatmap().BeatmapInfo));
        }

        private Func<Player> playToResults()
        {
            var player = playToCompletion();
            AddUntilStep("wait for results", () => (Game.ScreenStack.CurrentScreen as ResultsScreen)?.IsLoaded == true);
            return player;
        }

        private Func<Player> playToCompletion()
        {
            Player? player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("set mods", () => Game.SelectedMods.Value = new Mod[] { new OsuModNoFail(), new OsuModDoubleTime { SpeedChange = { Value = 2 } } });

            pushEnter();

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for track playing", () => beatmap().Track.IsRunning);
            AddStep("seek to near end", () => player.ChildrenOfType<GameplayClockContainer>().First().Seek(beatmap().Beatmap.HitObjects[^1].StartTime - 1000));
            AddUntilStep("wait for complete", () => player?.GameplayState.HasPassed, () => Is.True);

            return () => player!;
        }

        private void waitForScreen<T>() where T : OsuScreen =>
            AddUntilStep($"Wait for {typeof(T).ReadableName()}", () => Game.ScreenStack.CurrentScreen is T screen && screen.IsLoaded);

        private void pushEnter() =>
            AddStep("Press enter", () => InputManager.Key(Key.Enter));

        private void pushEscape() =>
            AddStep("Press escape", () => InputManager.Key(Key.Escape));
    }
}
