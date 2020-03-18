// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneScreenNavigation : OsuGameTestScene
    {
        private const float click_padding = 25;

        private Vector2 backButtonPosition => Game.ToScreenSpace(new Vector2(click_padding, Game.LayoutRectangle.Bottom - click_padding));

        private Vector2 optionsButtonPosition => Game.ToScreenSpace(new Vector2(click_padding, click_padding));

        [Test]
        public void TestExitSongSelectWithEscape()
        {
            TestSongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestSongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            pushEscape();
            AddAssert("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaEscapeAndConfirm();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player player = null;

            WorkingBeatmap beatmap() => Game.Beatmap.Value;
            Track track() => beatmap().Track;

            PushAndConfirm(() => new TestSongSelect());

            AddStep("import beatmap", () => ImportBeatmapTest.LoadOszIntoOsu(Game, virtualTrack: true).Wait());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            if (withUserPause)
                AddStep("pause", () => Game.Dependencies.Get<MusicController>().Stop());

            AddStep("press enter", () => pressAndRelease(Key.Enter));

            AddUntilStep("wait for player", () => (player = Game.ScreenStack.CurrentScreen as Player) != null);
            AddUntilStep("wait for fail", () => player.HasFailed);

            AddUntilStep("wait for track stop", () => !track().IsRunning);
            AddAssert("Ensure time before preview point", () => track().CurrentTime < beatmap().Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => track().IsRunning);
            AddAssert("Ensure time wasn't reset to preview point", () => track().CurrentTime < beatmap().Metadata.PreviewTime);
        }

        [Test]
        public void TestExitSongSelectWithClick()
        {
            TestSongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestSongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));

            // BackButton handles hover using its child button, so this checks whether or not any of BackButton's children are hovered.
            AddUntilStep("Back button is hovered", () => InputManager.HoveredDrawables.Any(d => d.Parent == Game.BackButton));

            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestExitMultiWithEscape()
        {
            PushAndConfirm(() => new Screens.Multi.Multiplayer());
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestExitMultiWithBackButton()
        {
            PushAndConfirm(() => new Screens.Multi.Multiplayer());
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestOpenOptionsAndExitWithEscape()
        {
            AddUntilStep("Wait for options to load", () => Game.Settings.IsLoaded);
            AddStep("Enter menu", () => pressAndRelease(Key.Enter));
            AddStep("Move mouse to options overlay", () => InputManager.MoveMouseTo(optionsButtonPosition));
            AddStep("Click options overlay", () => InputManager.Click(MouseButton.Left));
            AddAssert("Options overlay was opened", () => Game.Settings.State.Value == Visibility.Visible);
            AddStep("Hide options overlay using escape", () => pressAndRelease(Key.Escape));
            AddAssert("Options overlay was closed", () => Game.Settings.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestWaitForNextTrackInMenu()
        {
            bool trackCompleted = false;

            AddUntilStep("Wait for music controller", () => Game.MusicController.IsLoaded);
            AddStep("Seek close to end", () =>
            {
                Game.MusicController.SeekTo(Game.Beatmap.Value.Track.Length - 1000);
                Game.Beatmap.Value.Track.Completed += () => trackCompleted = true;
            });

            AddUntilStep("Track was completed", () => trackCompleted);
            AddUntilStep("Track was restarted", () => Game.Beatmap.Value.Track.IsRunning);
        }

        private void pushEscape() =>
            AddStep("Press escape", () => pressAndRelease(Key.Escape));

        private void exitViaEscapeAndConfirm()
        {
            pushEscape();
            ConfirmAtMainMenu();
        }

        private void exitViaBackButtonAndConfirm()
        {
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));
            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            ConfirmAtMainMenu();
        }

        private void pressAndRelease(Key key)
        {
            InputManager.PressKey(key);
            InputManager.ReleaseKey(key);
        }

        private class TestSongSelect : PlaySongSelect
        {
            public ModSelectOverlay ModSelectOverlay => ModSelect;
        }
    }
}
