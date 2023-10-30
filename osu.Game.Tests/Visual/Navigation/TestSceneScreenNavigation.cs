// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Screens.Select.Options;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneScreenNavigation : OsuGameTestScene
    {
        private const float click_padding = 25;

        private Vector2 backButtonPosition => Game.ToScreenSpace(new Vector2(click_padding, Game.LayoutRectangle.Bottom - click_padding));

        private Vector2 optionsButtonPosition => Game.ToScreenSpace(new Vector2(click_padding, click_padding));

        [TestCase(false)]
        [TestCase(true)]
        public void TestConfirmationRequiredToDiscardPlaylist(bool withPlaylistItemAdded)
        {
            Screens.OnlinePlay.Playlists.Playlists playlistScreen = null;

            AddUntilStep("wait for dialog overlay", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault() != null);

            PushAndConfirm(() => playlistScreen = new Screens.OnlinePlay.Playlists.Playlists());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddStep("open create screen", () =>
            {
                InputManager.MoveMouseTo(playlistScreen.ChildrenOfType<CreatePlaylistsRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            if (withPlaylistItemAdded)
            {
                AddUntilStep("wait for settings displayed",
                    () => (playlistScreen.CurrentSubScreen as PlaylistsRoomSubScreen)?.ChildrenOfType<PlaylistsRoomSettingsOverlay>().SingleOrDefault()?.State.Value == Visibility.Visible);

                AddStep("edit playlist", () => InputManager.Key(Key.Enter));

                AddUntilStep("wait for song select", () => (playlistScreen.CurrentSubScreen as PlaylistsSongSelect)?.BeatmapSetsLoaded == true);

                AddUntilStep("wait for selection", () => !Game.Beatmap.IsDefault);

                AddStep("add item", () => InputManager.Key(Key.Enter));

                AddUntilStep("wait for return to playlist screen", () => playlistScreen.CurrentSubScreen is PlaylistsRoomSubScreen);

                AddStep("go back to song select", () =>
                {
                    InputManager.MoveMouseTo(playlistScreen.ChildrenOfType<PurpleRoundedButton>().Single(b => b.Text == "Edit playlist"));
                    InputManager.Click(MouseButton.Left);
                });

                AddUntilStep("wait for song select", () => (playlistScreen.CurrentSubScreen as PlaylistsSongSelect)?.BeatmapSetsLoaded == true);

                AddStep("press home button", () =>
                {
                    InputManager.MoveMouseTo(Game.Toolbar.ChildrenOfType<ToolbarHomeButton>().Single());
                    InputManager.Click(MouseButton.Left);
                });

                AddAssert("confirmation dialog shown", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is not null);

                pushEscape();
                pushEscape();

                AddAssert("confirmation dialog shown", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is not null);

                AddStep("confirm exit", () => InputManager.Key(Key.Enter));

                AddAssert("dialog dismissed", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog == null);

                exitViaEscapeAndConfirm();
            }
            else
            {
                pushEscape();
                AddAssert("confirmation dialog not shown", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog == null);

                exitViaEscapeAndConfirm();
            }
        }

        [Test]
        public void TestExitSongSelectWithEscape()
        {
            TestPlaySongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            pushEscape();
            AddAssert("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestSongSelectBackActionHandling()
        {
            TestPlaySongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestPlaySongSelect());

            AddStep("set filter", () => songSelect.ChildrenOfType<SearchTextBox>().Single().Current.Value = "test");
            AddStep("press back", () => InputManager.Click(MouseButton.Button1));

            AddAssert("still at song select", () => Game.ScreenStack.CurrentScreen == songSelect);
            AddAssert("filter cleared", () => string.IsNullOrEmpty(songSelect.ChildrenOfType<SearchTextBox>().Single().Current.Value));

            AddStep("set filter again", () => songSelect.ChildrenOfType<SearchTextBox>().Single().Current.Value = "test");
            AddStep("open collections dropdown", () =>
            {
                InputManager.MoveMouseTo(songSelect.ChildrenOfType<CollectionDropdown>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("press back once", () => InputManager.Click(MouseButton.Button1));
            AddAssert("still at song select", () => Game.ScreenStack.CurrentScreen == songSelect);
            AddAssert("collections dropdown closed", () => songSelect
                                                           .ChildrenOfType<CollectionDropdown>().Single()
                                                           .ChildrenOfType<Dropdown<CollectionFilterMenuItem>.DropdownMenu>().Single().State == MenuState.Closed);

            AddStep("press back a second time", () => InputManager.Click(MouseButton.Button1));
            AddAssert("filter cleared", () => string.IsNullOrEmpty(songSelect.ChildrenOfType<SearchTextBox>().Single().Current.Value));

            AddStep("press back a third time", () => InputManager.Click(MouseButton.Button1));
            ConfirmAtMainMenu();
        }

        [Test]
        public void TestSongSelectScrollHandling()
        {
            TestPlaySongSelect songSelect = null;
            double scrollPosition = 0;

            AddStep("set game volume to max", () => Game.Dependencies.Get<FrameworkConfigManager>().SetValue(FrameworkSetting.VolumeUniversal, 1d));
            AddUntilStep("wait for volume overlay to hide", () => Game.ChildrenOfType<VolumeOverlay>().SingleOrDefault()?.State.Value, () => Is.EqualTo(Visibility.Hidden));
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);
            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());
            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("store scroll position", () => scrollPosition = getCarouselScrollPosition());

            AddStep("move to left side", () => InputManager.MoveMouseTo(
                songSelect.ChildrenOfType<Screens.Select.SongSelect.LeftSideInteractionContainer>().Single().ScreenSpaceDrawQuad.TopLeft + new Vector2(1)));
            AddStep("scroll down", () => InputManager.ScrollVerticalBy(-1));
            AddAssert("carousel didn't move", getCarouselScrollPosition, () => Is.EqualTo(scrollPosition));

            AddRepeatStep("alt-scroll down", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.ScrollVerticalBy(-1);
                InputManager.ReleaseKey(Key.AltLeft);
            }, 5);
            AddAssert("game volume decreased", () => Game.Dependencies.Get<FrameworkConfigManager>().Get<double>(FrameworkSetting.VolumeUniversal), () => Is.LessThan(1));

            AddStep("move to carousel", () => InputManager.MoveMouseTo(songSelect.ChildrenOfType<BeatmapCarousel>().Single()));
            AddStep("scroll down", () => InputManager.ScrollVerticalBy(-1));
            AddAssert("carousel moved", getCarouselScrollPosition, () => Is.Not.EqualTo(scrollPosition));

            double getCarouselScrollPosition() => Game.ChildrenOfType<UserTrackingScrollContainer<DrawableCarouselItem>>().Single().Current;
        }

        /// <summary>
        /// This tests that the F1 key will open the mod select overlay, and not be handled / blocked by the music controller (which has the same default binding
        /// but should be handled *after* song select).
        /// </summary>
        [Test]
        public void TestOpenModSelectOverlayUsingAction()
        {
            TestPlaySongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddStep("Show mods overlay", () => InputManager.Key(Key.F1));
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestRetryCountIncrements()
        {
            Player player = null;

            Screens.Select.SongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                player = Game.ScreenStack.CurrentScreen as Player;
                return player?.IsLoaded == true;
            });

            AddAssert("retry count is 0", () => player.RestartCount == 0);

            // todo: see https://github.com/ppy/osu/issues/22220
            // tests are supposed to be immune to this edge case by the logic in TestPlayer,
            // but we're running a full game instance here, so we have to work around it manually.
            AddStep("end spectator before retry", () => Game.SpectatorClient.EndPlaying(player.GameplayState));

            AddStep("attempt to retry", () => player.ChildrenOfType<HotkeyRetryOverlay>().First().Action());
            AddAssert("old player score marked failed", () => player.Score.ScoreInfo.Rank, () => Is.EqualTo(ScoreRank.F));
            AddUntilStep("wait for old player gone", () => Game.ScreenStack.CurrentScreen != player);

            AddUntilStep("get new player", () => (player = Game.ScreenStack.CurrentScreen as Player) != null);
            AddAssert("retry count is 1", () => player.RestartCount == 1);
        }

        [Test]
        public void TestRetryImmediatelyAfterCompletion()
        {
            var getOriginalPlayer = playToCompletion();

            AddStep("attempt to retry", () => getOriginalPlayer().ChildrenOfType<HotkeyRetryOverlay>().First().Action());
            AddAssert("original play isn't failed", () => getOriginalPlayer().Score.ScoreInfo.Rank, () => Is.Not.EqualTo(ScoreRank.F));
            AddUntilStep("wait for player", () => Game.ScreenStack.CurrentScreen != getOriginalPlayer() && Game.ScreenStack.CurrentScreen is Player);
        }

        [Test]
        public void TestExitImmediatelyAfterCompletion()
        {
            var player = playToCompletion();

            AddStep("attempt to exit", () => player().ChildrenOfType<HotkeyExitOverlay>().First().Action());
            AddUntilStep("wait for results", () => Game.ScreenStack.CurrentScreen is ResultsScreen);
        }

        [Test]
        public void TestRetryFromResults()
        {
            var getOriginalPlayer = playToResults();

            AddStep("attempt to retry", () => ((ResultsScreen)Game.ScreenStack.CurrentScreen).ChildrenOfType<HotkeyRetryOverlay>().First().Action());
            AddUntilStep("wait for player", () => Game.ScreenStack.CurrentScreen != getOriginalPlayer() && Game.ScreenStack.CurrentScreen is Player);
        }

        [Test]
        public void TestDeleteAllScoresAfterPlaying()
        {
            playToResults();

            ScoreInfo score = null;
            LeaderboardScore scorePanel = null;

            AddStep("get score", () => score = ((ResultsScreen)Game.ScreenStack.CurrentScreen).Score);

            AddAssert("ensure score is databased", () => Game.Realm.Run(r => r.Find<ScoreInfo>(score.ID)?.DeletePending == false));

            AddStep("press back button", () => Game.ChildrenOfType<BackButton>().First().Action());

            AddStep("show local scores",
                () => Game.ChildrenOfType<BeatmapDetailAreaTabControl>().First().Current.Value = new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Local));

            AddUntilStep("wait for score displayed", () => (scorePanel = Game.ChildrenOfType<LeaderboardScore>().FirstOrDefault(s => s.Score.Equals(score))) != null);

            AddStep("open options", () => InputManager.Key(Key.F3));

            AddStep("choose clear all scores", () => InputManager.Key(Key.Number4));

            AddUntilStep("wait for dialog display", () => ((Drawable)Game.Dependencies.Get<IDialogOverlay>()).IsLoaded);
            AddUntilStep("wait for dialog", () => Game.Dependencies.Get<IDialogOverlay>().CurrentDialog != null);
            AddStep("confirm deletion", () => InputManager.Key(Key.Number1));
            AddUntilStep("wait for dialog dismissed", () => Game.Dependencies.Get<IDialogOverlay>().CurrentDialog == null);

            AddUntilStep("ensure score is pending deletion", () => Game.Realm.Run(r => r.Find<ScoreInfo>(score.ID)?.DeletePending == true));

            AddUntilStep("wait for score panel removal", () => scorePanel.Parent == null);
        }

        [Test]
        public void TestDeleteScoreAfterPlaying()
        {
            playToResults();

            ScoreInfo score = null;
            LeaderboardScore scorePanel = null;

            AddStep("get score", () => score = ((ResultsScreen)Game.ScreenStack.CurrentScreen).Score);

            AddAssert("ensure score is databased", () => Game.Realm.Run(r => r.Find<ScoreInfo>(score.ID)?.DeletePending == false));

            AddStep("press back button", () => Game.ChildrenOfType<BackButton>().First().Action());

            AddStep("show local scores",
                () => Game.ChildrenOfType<BeatmapDetailAreaTabControl>().First().Current.Value = new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Local));

            AddUntilStep("wait for score displayed", () => (scorePanel = Game.ChildrenOfType<LeaderboardScore>().FirstOrDefault(s => s.Score.Equals(score))) != null);

            AddStep("right click panel", () =>
            {
                InputManager.MoveMouseTo(scorePanel);
                InputManager.Click(MouseButton.Right);
            });

            AddStep("click delete", () =>
            {
                var dropdownItem = Game
                                   .ChildrenOfType<PlayBeatmapDetailArea>().First()
                                   .ChildrenOfType<OsuContextMenu>().First()
                                   .ChildrenOfType<DrawableOsuMenuItem>().First(i => i.Item.Text.ToString() == "Delete");

                InputManager.MoveMouseTo(dropdownItem);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for dialog display", () => ((Drawable)Game.Dependencies.Get<IDialogOverlay>()).IsLoaded);
            AddUntilStep("wait for dialog", () => Game.Dependencies.Get<IDialogOverlay>().CurrentDialog != null);
            AddStep("confirm deletion", () => InputManager.Key(Key.Number1));
            AddUntilStep("wait for dialog dismissed", () => Game.Dependencies.Get<IDialogOverlay>().CurrentDialog == null);

            AddUntilStep("ensure score is pending deletion", () => Game.Realm.Run(r => r.Find<ScoreInfo>(score.ID)?.DeletePending == true));

            AddUntilStep("wait for score panel removal", () => scorePanel.Parent == null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            Screens.Select.SongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);

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

            AddUntilStep("wait for fail", () => player.GameplayState.HasFailed);

            AddUntilStep("wait for track stop", () => !Game.MusicController.IsPlaying);
            AddAssert("Ensure time before preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => Game.MusicController.IsPlaying);
            AddAssert("Ensure time wasn't reset to preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);
        }

        [Test]
        public void TestMenuMakesMusic()
        {
            TestPlaySongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestPlaySongSelect());

            AddUntilStep("wait for no track", () => Game.MusicController.CurrentTrack.IsDummyDevice);

            AddStep("return to menu", () => songSelect.Exit());

            AddUntilStep("wait for track", () => !Game.MusicController.CurrentTrack.IsDummyDevice && Game.MusicController.IsPlaying);
        }

        [Test]
        public void TestPushSongSelectAndPressBackButtonImmediately()
        {
            AddStep("push song select", () => Game.ScreenStack.Push(new TestPlaySongSelect()));
            AddStep("press back button", () => Game.ChildrenOfType<BackButton>().First().Action());
            AddWaitStep("wait two frames", 2);
        }

        [Test]
        public void TestExitSongSelectWithClick()
        {
            TestPlaySongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);

            AddStep("Move mouse to dimmed area", () => InputManager.MoveMouseTo(new Vector2(
                songSelect.ScreenSpaceDrawQuad.TopLeft.X + 1,
                songSelect.ScreenSpaceDrawQuad.TopLeft.Y + songSelect.ScreenSpaceDrawQuad.Height / 2)));
            AddStep("Click left mouse button", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestModsResetOnEnteringMultiplayer()
        {
            var osuAutomationMod = new OsuModAutoplay();

            AddStep("Enable autoplay", () => { Game.SelectedMods.Value = new[] { osuAutomationMod }; });

            PushAndConfirm(() => new Screens.OnlinePlay.Multiplayer.Multiplayer());
            AddUntilStep("Mods are removed", () => Game.SelectedMods.Value.Count == 0);

            AddStep("Return to menu", () => Game.ScreenStack.CurrentScreen.Exit());
            AddUntilStep("Mods are restored", () => Game.SelectedMods.Value.Contains(osuAutomationMod));
        }

        [Test]
        public void TestExitMultiWithEscape()
        {
            PushAndConfirm(() => new Screens.OnlinePlay.Playlists.Playlists());
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestExitMultiWithBackButton()
        {
            PushAndConfirm(() => new Screens.OnlinePlay.Playlists.Playlists());
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestOpenOptionsAndExitWithEscape()
        {
            AddUntilStep("Wait for options to load", () => Game.Settings.IsLoaded);
            AddStep("Enter menu", () => InputManager.Key(Key.Enter));
            AddStep("Move mouse to options overlay", () => InputManager.MoveMouseTo(optionsButtonPosition));
            AddStep("Click options overlay", () => InputManager.Click(MouseButton.Left));
            AddAssert("Options overlay was opened", () => Game.Settings.State.Value == Visibility.Visible);
            AddStep("Hide options overlay using escape", () => InputManager.Key(Key.Escape));
            AddAssert("Options overlay was closed", () => Game.Settings.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestWaitForNextTrackInMenu()
        {
            bool trackCompleted = false;

            AddUntilStep("Wait for music controller", () => Game.MusicController.IsLoaded);
            AddStep("Seek close to end", () =>
            {
                Game.MusicController.SeekTo(Game.MusicController.CurrentTrack.Length - 1000);
                Game.MusicController.CurrentTrack.Completed += () => trackCompleted = true;
            });

            AddUntilStep("Track was completed", () => trackCompleted);
            AddUntilStep("Track was restarted", () => Game.MusicController.IsPlaying);
        }

        [Test]
        public void TestModSelectInput()
        {
            AddUntilStep("Wait for toolbar to load", () => Game.Toolbar.IsLoaded);

            TestPlaySongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());

            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());

            AddStep("Change ruleset to osu!taiko", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Number2);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("Ruleset changed to osu!taiko", () => Game.Toolbar.ChildrenOfType<ToolbarRulesetSelector>().Single().Current.Value.OnlineID == 1);

            AddAssert("Mods overlay still visible", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestBeatmapOptionsInput()
        {
            AddUntilStep("Wait for toolbar to load", () => Game.Toolbar.IsLoaded);

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            TestPlaySongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());

            AddStep("Show options overlay", () => songSelect.BeatmapOptionsOverlay.Show());

            AddStep("Change ruleset to osu!taiko", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Number2);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("Ruleset changed to osu!taiko", () => Game.Toolbar.ChildrenOfType<ToolbarRulesetSelector>().Single().Current.Value.OnlineID == 1);

            AddAssert("Options overlay still visible", () => songSelect.BeatmapOptionsOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestSettingsViaHotkeyFromMainMenu()
        {
            AddUntilStep("Wait for toolbar to load", () => Game.Toolbar.IsLoaded);

            AddAssert("toolbar not displayed", () => Game.Toolbar.State.Value == Visibility.Hidden);

            AddStep("press settings hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.O);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("settings displayed", () => Game.Settings.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestToolbarHiddenByUser()
        {
            AddUntilStep("Wait for toolbar to load", () => Game.Toolbar.IsLoaded);

            AddStep("Enter menu", () => InputManager.Key(Key.Enter));
            AddUntilStep("Toolbar is visible", () => Game.Toolbar.State.Value == Visibility.Visible);

            AddStep("Hide toolbar", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.T);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            pushEscape();

            AddStep("Enter menu", () => InputManager.Key(Key.Enter));

            AddAssert("Toolbar is hidden", () => Game.Toolbar.State.Value == Visibility.Hidden);

            AddStep("Enter song select", () =>
            {
                InputManager.Key(Key.Enter);
                InputManager.Key(Key.Enter);
            });

            AddAssert("Toolbar is hidden", () => Game.Toolbar.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPushMatchSubScreenAndPressBackButtonImmediately()
        {
            TestMultiplayerComponents multiplayerComponents = null;

            PushAndConfirm(() => multiplayerComponents = new TestMultiplayerComponents());

            AddUntilStep("wait for lounge", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().Single().Open());
            AddStep("press back button", () => Game.ChildrenOfType<BackButton>().First().Action());
            AddWaitStep("wait two frames", 2);

            AddStep("exit lounge", () => Game.ScreenStack.Exit());
            // `TestMultiplayerComponents` registers a request handler in its BDL, but never unregisters it.
            // to prevent the handler living for longer than it should be, clean up manually.
            AddStep("clean up multiplayer request handler", () => ((DummyAPIAccess)API).HandleRequest = null);
        }

        [Test]
        public void TestFeaturedArtistDisclaimerDialog()
        {
            BeatmapListingOverlay getBeatmapListingOverlay() => Game.ChildrenOfType<BeatmapListingOverlay>().FirstOrDefault();

            AddStep("Wait for notifications to load", () => Game.SearchBeatmapSet(string.Empty));
            AddUntilStep("wait for dialog overlay", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault() != null);

            AddUntilStep("Wait for beatmap overlay to load", () => getBeatmapListingOverlay()?.State.Value == Visibility.Visible);
            AddAssert("featured artist filter is on", () => getBeatmapListingOverlay().ChildrenOfType<BeatmapSearchGeneralFilterRow>().First().Current.Contains(SearchGeneral.FeaturedArtists));
            AddStep("toggle featured artist filter",
                () => getBeatmapListingOverlay().ChildrenOfType<FilterTabItem<SearchGeneral>>().First(i => i.Value == SearchGeneral.FeaturedArtists).TriggerClick());

            AddAssert("disclaimer dialog is shown", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog != null);
            AddAssert("featured artist filter is still on", () => getBeatmapListingOverlay().ChildrenOfType<BeatmapSearchGeneralFilterRow>().First().Current.Contains(SearchGeneral.FeaturedArtists));

            AddStep("confirm", () => InputManager.Key(Key.Enter));
            AddAssert("dialog dismissed", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog == null);

            AddUntilStep("featured artist filter is off", () => !getBeatmapListingOverlay().ChildrenOfType<BeatmapSearchGeneralFilterRow>().First().Current.Contains(SearchGeneral.FeaturedArtists));
        }

        [Test]
        public void TestBeatmapListingLinkSearchOnInitialOpen()
        {
            BeatmapListingOverlay getBeatmapListingOverlay() => Game.ChildrenOfType<BeatmapListingOverlay>().FirstOrDefault();

            AddStep("open beatmap overlay with test query", () => Game.SearchBeatmapSet("test"));

            AddUntilStep("wait for beatmap overlay to load", () => getBeatmapListingOverlay()?.State.Value == Visibility.Visible);

            AddAssert("beatmap overlay sorted by relevance", () => getBeatmapListingOverlay().ChildrenOfType<BeatmapListingSortTabControl>().Single().Current.Value == SortCriteria.Relevance);
        }

        [Test]
        public void TestMainOverlaysClosesNotificationOverlay()
        {
            ChangelogOverlay getChangelogOverlay() => Game.ChildrenOfType<ChangelogOverlay>().FirstOrDefault();

            AddUntilStep("Wait for notifications to load", () => Game.Notifications.IsLoaded);
            AddStep("Show notifications", () => Game.Notifications.Show());
            AddUntilStep("wait for notifications shown", () => Game.Notifications.IsPresent && Game.Notifications.State.Value == Visibility.Visible);
            AddStep("Show changelog listing", () => Game.ShowChangelogListing());
            AddUntilStep("wait for changelog shown", () => getChangelogOverlay()?.IsPresent == true && getChangelogOverlay()?.State.Value == Visibility.Visible);
            AddAssert("Notifications is hidden", () => Game.Notifications.State.Value == Visibility.Hidden);

            AddStep("Show notifications", () => Game.Notifications.Show());
            AddUntilStep("wait for notifications shown", () => Game.Notifications.State.Value == Visibility.Visible);
            AddUntilStep("changelog still visible", () => getChangelogOverlay().State.Value == Visibility.Visible);
        }

        [Test]
        public void TestMainOverlaysClosesSettingsOverlay()
        {
            ChangelogOverlay getChangelogOverlay() => Game.ChildrenOfType<ChangelogOverlay>().FirstOrDefault();

            AddUntilStep("Wait for settings to load", () => Game.Settings.IsLoaded);
            AddStep("Show settings", () => Game.Settings.Show());
            AddUntilStep("wait for settings shown", () => Game.Settings.IsPresent && Game.Settings.State.Value == Visibility.Visible);
            AddStep("Show changelog listing", () => Game.ShowChangelogListing());
            AddUntilStep("wait for changelog shown", () => getChangelogOverlay()?.IsPresent == true && getChangelogOverlay()?.State.Value == Visibility.Visible);
            AddAssert("Settings is hidden", () => Game.Settings.State.Value == Visibility.Hidden);

            AddStep("Show settings", () => Game.Settings.Show());
            AddUntilStep("wait for settings shown", () => Game.Settings.State.Value == Visibility.Visible);
            AddUntilStep("changelog still visible", () => getChangelogOverlay().State.Value == Visibility.Visible);
        }

        [Test]
        public void TestOverlayClosing()
        {
            // use now playing overlay for "overlay -> background" drag case
            // since most overlays use a scroll container that absorbs on mouse down
            NowPlayingOverlay nowPlayingOverlay = null;

            AddUntilStep("Wait for now playing load", () => (nowPlayingOverlay = Game.ChildrenOfType<NowPlayingOverlay>().FirstOrDefault()) != null);

            AddStep("enter menu", () => InputManager.Key(Key.Enter));
            AddUntilStep("toolbar displayed", () => Game.Toolbar.State.Value == Visibility.Visible);

            AddStep("open now playing", () => InputManager.Key(Key.F6));
            AddUntilStep("now playing is visible", () => nowPlayingOverlay.State.Value == Visibility.Visible);

            // drag tests

            // background -> toolbar
            AddStep("move cursor to background", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomRight));
            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move cursor to toolbar", () => InputManager.MoveMouseTo(Game.Toolbar.ScreenSpaceDrawQuad.Centre));
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("now playing is hidden", () => nowPlayingOverlay.State.Value == Visibility.Hidden);

            AddStep("press now playing hotkey", () => InputManager.Key(Key.F6));

            // toolbar -> background
            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move cursor to background", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomRight));
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("now playing is still visible", () => nowPlayingOverlay.State.Value == Visibility.Visible);

            // background -> overlay
            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move cursor to now playing overlay", () => InputManager.MoveMouseTo(nowPlayingOverlay.ScreenSpaceDrawQuad.Centre));
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("now playing is still visible", () => nowPlayingOverlay.State.Value == Visibility.Visible);

            // overlay -> background
            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move cursor to background", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomRight));
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("now playing is still visible", () => nowPlayingOverlay.State.Value == Visibility.Visible);

            // background -> background
            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move cursor to left", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomLeft));
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("now playing is hidden", () => nowPlayingOverlay.State.Value == Visibility.Hidden);

            AddStep("press now playing hotkey", () => InputManager.Key(Key.F6));

            // click tests

            // toolbar
            AddStep("move cursor to toolbar", () => InputManager.MoveMouseTo(Game.Toolbar.ScreenSpaceDrawQuad.Centre));
            AddStep("click left mouse button", () => InputManager.Click(MouseButton.Left));
            AddAssert("now playing is still visible", () => nowPlayingOverlay.State.Value == Visibility.Visible);

            // background
            AddStep("move cursor to background", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.BottomRight));
            AddStep("click left mouse button", () => InputManager.Click(MouseButton.Left));
            AddAssert("now playing is hidden", () => nowPlayingOverlay.State.Value == Visibility.Hidden);

            // move the mouse firmly inside game bounds to avoid interfering with other tests.
            AddStep("center cursor", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
        }

        [Test]
        public void TestExitWithOperationInProgress()
        {
            AddUntilStep("wait for dialog overlay", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault() != null);

            ProgressNotification progressNotification = null!;

            AddStep("start ongoing operation", () =>
            {
                progressNotification = new ProgressNotification
                {
                    Text = "Something is still running",
                    Progress = 0.5f,
                    State = ProgressNotificationState.Active,
                };
                Game.Notifications.Post(progressNotification);
            });

            AddStep("Hold escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("confirmation dialog shown", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is ConfirmExitDialog);
            AddStep("Release escape", () => InputManager.ReleaseKey(Key.Escape));

            AddStep("cancel exit", () => InputManager.Key(Key.Escape));
            AddAssert("dialog dismissed", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog == null);

            AddStep("complete operation", () =>
            {
                progressNotification.Progress = 100;
                progressNotification.State = ProgressNotificationState.Completed;
            });

            AddStep("Hold escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("Wait for intro", () => Game.ScreenStack.CurrentScreen is IntroScreen);
            AddStep("Release escape", () => InputManager.ReleaseKey(Key.Escape));

            AddUntilStep("Wait for game exit", () => Game.ScreenStack.CurrentScreen == null);
        }

        [Test]
        public void TestForceExitWithOperationInProgress()
        {
            AddStep("set hold delay to 0", () => Game.LocalConfig.SetValue(OsuSetting.UIHoldActivationDelay, 0.0));
            AddUntilStep("wait for dialog overlay", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault() != null);

            AddStep("start ongoing operation", () =>
            {
                Game.Notifications.Post(new ProgressNotification
                {
                    Text = "Something is still running",
                    Progress = 0.5f,
                    State = ProgressNotificationState.Active,
                });
            });

            AddStep("attempt exit", () =>
            {
                for (int i = 0; i < 2; ++i)
                    Game.ScreenStack.CurrentScreen.Exit();
            });
            AddUntilStep("stopped at exit confirm", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is ConfirmExitDialog);
        }

        [Test]
        public void TestExitGameFromSongSelect()
        {
            PushAndConfirm(() => new TestPlaySongSelect());
            exitViaEscapeAndConfirm();

            pushEscape(); // returns to osu! logo

            AddStep("Hold escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("Wait for intro", () => Game.ScreenStack.CurrentScreen is IntroScreen);
            AddStep("Release escape", () => InputManager.ReleaseKey(Key.Escape));
            AddUntilStep("Wait for game exit", () => Game.ScreenStack.CurrentScreen == null);
            AddStep("test dispose doesn't crash", () => Game.Dispose());
        }

        [Test]
        public void TestExitWithHoldDisabled()
        {
            AddStep("set hold delay to 0", () => Game.LocalConfig.SetValue(OsuSetting.UIHoldActivationDelay, 0.0));

            AddStep("press escape twice rapidly", () =>
            {
                InputManager.Key(Key.Escape);
                Schedule(InputManager.Key, Key.Escape);
            });

            pushEscape();

            AddAssert("exit dialog is shown", () => Game.Dependencies.Get<IDialogOverlay>().CurrentDialog is ConfirmExitDialog);
        }

        [Test]
        public void TestTouchScreenDetection()
        {
            AddStep("touch logo", () =>
            {
                var button = Game.ChildrenOfType<OsuLogo>().Single();
                var touch = new Touch(TouchSource.Touch1, button.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch screen detected active", () => Game.Dependencies.Get<SessionStatics>().Get<bool>(Static.TouchInputActive), () => Is.True);

            AddStep("click settings button", () =>
            {
                var button = Game.ChildrenOfType<MainMenuButton>().Last();
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("touch screen detected inactive", () => Game.Dependencies.Get<SessionStatics>().Get<bool>(Static.TouchInputActive), () => Is.False);
        }

        private Func<Player> playToResults()
        {
            var player = playToCompletion();
            AddUntilStep("wait for results", () => (Game.ScreenStack.CurrentScreen as ResultsScreen)?.IsLoaded == true);
            return player;
        }

        private Func<Player> playToCompletion()
        {
            Player player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            Screens.Select.SongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("set mods", () => Game.SelectedMods.Value = new Mod[] { new OsuModNoFail(), new OsuModDoubleTime { SpeedChange = { Value = 2 } } });

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for track playing", () => beatmap().Track.IsRunning);
            AddStep("seek to near end", () => player.ChildrenOfType<GameplayClockContainer>().First().Seek(beatmap().Beatmap.HitObjects[^1].StartTime - 1000));
            AddUntilStep("wait for complete", () => player.GameplayState.HasPassed);

            return () => player;
        }

        private void pushEscape() =>
            AddStep("Press escape", () => InputManager.Key(Key.Escape));

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

        public partial class TestPlaySongSelect : PlaySongSelect
        {
            public ModSelectOverlay ModSelectOverlay => ModSelect;

            public BeatmapOptionsOverlay BeatmapOptionsOverlay => BeatmapOptions;
        }
    }
}
