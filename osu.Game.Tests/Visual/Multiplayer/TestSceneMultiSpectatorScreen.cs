// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiSpectatorScreen : MultiplayerTestScene
    {
        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        private MultiSpectatorScreen spectatorScreen = null!;

        private readonly List<MultiplayerRoomUser> playingUsers = new List<MultiplayerRoomUser>();

        private BeatmapSetInfo importedSet = null!;
        private BeatmapInfo importedBeatmap = null!;

        private int importedBeatmapId;

        [BackgroundDependencyLoader]
        private void load()
        {
            importedSet = BeatmapImportHelper.LoadOszIntoOsu(game, virtualTrack: true).GetResultSafely();
            importedBeatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0);
            importedBeatmapId = importedBeatmap.OnlineID;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("clear playing users", () => playingUsers.Clear());
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(9)]
        public void TestGeneral(int count)
        {
            int[] userIds = getPlayerIds(count);

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [TestCase(2)]
        [TestCase(16)]
        public void TestTeams(int count)
        {
            int[] userIds = getPlayerIds(count);

            start(userIds, teams: true);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestMultipleStartRequests()
        {
            int[] userIds = getPlayerIds(2);

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);

            start(userIds);
        }

        [Test]
        public void TestDelayedStart()
        {
            AddStep("start players silently", () =>
            {
                OnlinePlayDependencies.MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID }, true);
                OnlinePlayDependencies.MultiplayerClient.AddUser(new APIUser { Id = PLAYER_2_ID }, true);

                playingUsers.Add(new MultiplayerRoomUser(PLAYER_1_ID));
                playingUsers.Add(new MultiplayerRoomUser(PLAYER_2_ID));
            });

            loadSpectateScreen(false);

            AddWaitStep("wait a bit", 10);
            AddStep("load player first_player_id", () => SpectatorClient.SendStartPlay(PLAYER_1_ID, importedBeatmapId));
            AddUntilStep("one player added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 1);

            AddWaitStep("wait a bit", 10);
            AddStep("load player second_player_id", () => SpectatorClient.SendStartPlay(PLAYER_2_ID, importedBeatmapId));
            AddUntilStep("two players added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 2);
        }

        [Test]
        public void TestSpectatorPlayerInteractiveElementsHidden()
        {
            HUDVisibilityMode originalConfigValue = default;

            AddStep("get original config hud visibility", () => originalConfigValue = config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));
            AddStep("set config hud visibility to always", () => config.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always));

            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen(false);

            AddUntilStep("wait for player loaders", () => this.ChildrenOfType<PlayerLoader>().Count() == 2);
            AddAssert("all player loader settings hidden", () => this.ChildrenOfType<PlayerLoader>().All(l => !l.ChildrenOfType<FillFlowContainer<PlayerSettingsGroup>>().Any()));

            AddUntilStep("wait for players to load", () => spectatorScreen.AllPlayersLoaded);

            // components wrapped in skinnable target containers load asynchronously, potentially taking more than one frame to load.
            // therefore use until step rather than direct assert to account for that.
            AddUntilStep("all interactive elements removed", () => this.ChildrenOfType<Player>().All(p =>
                !p.ChildrenOfType<PlayerSettingsOverlay>().Any() &&
                !p.ChildrenOfType<HoldForMenuButton>().Any() &&
                p.ChildrenOfType<ArgonSongProgressBar>().SingleOrDefault()?.Interactive == false));

            AddStep("restore config hud visibility", () => config.SetValue(OsuSetting.HUDVisibilityMode, originalConfigValue));
        }

        [Test]
        public void TestTeamDisplay()
        {
            AddStep("start players", () =>
            {
                var player1 = OnlinePlayDependencies.MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID }, true);
                player1.MatchState = new TeamVersusUserState
                {
                    TeamID = 0,
                };

                var player2 = OnlinePlayDependencies.MultiplayerClient.AddUser(new APIUser { Id = PLAYER_2_ID }, true);
                player2.MatchState = new TeamVersusUserState
                {
                    TeamID = 1,
                };

                SpectatorClient.SendStartPlay(player1.UserID, importedBeatmapId);
                SpectatorClient.SendStartPlay(player2.UserID, importedBeatmapId);

                playingUsers.Add(player1);
                playingUsers.Add(player2);
            });

            loadSpectateScreen();

            sendFrames(PLAYER_1_ID, 1000);
            sendFrames(PLAYER_2_ID, 1000);

            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestTimeDoesNotProgressWhileAllPlayersPaused()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            sendFrames(PLAYER_1_ID, 40);
            sendFrames(PLAYER_2_ID, 20);

            waitUntilPaused(PLAYER_2_ID);
            checkRunningInstant(PLAYER_1_ID);
            AddAssert("master clock still running", () => this.ChildrenOfType<MasterGameplayClockContainer>().Single().IsRunning);

            waitUntilPaused(PLAYER_1_ID);
            AddUntilStep("master clock paused", () => !this.ChildrenOfType<MasterGameplayClockContainer>().Single().IsRunning);
        }

        [Test]
        public void TestPlayersMustStartSimultaneously()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(PLAYER_1_ID, 20);
            checkPausedInstant(PLAYER_1_ID);
            checkPausedInstant(PLAYER_2_ID);

            // Send frames for the other player, both should now start playing.
            sendFrames(PLAYER_2_ID, 20);
            checkRunningInstant(PLAYER_1_ID);
            checkRunningInstant(PLAYER_2_ID);
        }

        [Test]
        public void TestPlayersDoNotStartSimultaneouslyIfBufferingForMaximumStartDelay()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(PLAYER_1_ID, 1000);
            checkPausedInstant(PLAYER_1_ID);
            checkPausedInstant(PLAYER_2_ID);

            // Wait for the start delay seconds...
            AddWaitStep("wait maximum start delay seconds", (int)(SpectatorSyncManager.MAXIMUM_START_DELAY / TimePerAction));

            // Player 1 should start playing by itself, player 2 should remain paused.
            checkRunningInstant(PLAYER_1_ID);
            checkPausedInstant(PLAYER_2_ID);
        }

        [Test]
        public void TestPlayersContinueWhileOthersBuffer()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(PLAYER_1_ID, 20);
            sendFrames(PLAYER_2_ID);
            checkRunningInstant(PLAYER_1_ID);
            checkRunningInstant(PLAYER_2_ID);

            // Eventually player 2 will pause, player 1 must remain running.
            waitUntilPaused(PLAYER_2_ID);
            checkRunningInstant(PLAYER_1_ID);

            // Eventually both players will run out of frames and should pause.
            waitUntilPaused(PLAYER_1_ID);
            checkPausedInstant(PLAYER_2_ID);

            // Send more frames for the first player only. Player 1 should start playing with player 2 remaining paused.
            sendFrames(PLAYER_1_ID, 20);
            checkPausedInstant(PLAYER_2_ID);
            checkRunningInstant(PLAYER_1_ID);

            // Send more frames for the second player. Both should be playing
            sendFrames(PLAYER_2_ID, 20);
            checkRunningInstant(PLAYER_2_ID);
            checkRunningInstant(PLAYER_1_ID);
        }

        [Test]
        public void TestPlayersCatchUpAfterFallingBehind()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(PLAYER_1_ID, 1000);
            sendFrames(PLAYER_2_ID, 30);
            checkRunningInstant(PLAYER_1_ID);
            checkRunningInstant(PLAYER_2_ID);

            // Eventually player 2 will run out of frames and should pause.
            waitUntilPaused(PLAYER_2_ID);
            AddWaitStep("wait a few more frames", 10);

            // Send more frames for player 2. It should unpause.
            sendFrames(PLAYER_2_ID, 1000);
            checkRunningInstant(PLAYER_2_ID);

            // Player 2 should catch up to player 1 after unpausing.
            waitForCatchup(PLAYER_2_ID);
            AddWaitStep("wait a bit", 10);
        }

        [Test]
        public void TestMostInSyncUserIsAudioSource()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // With no frames, the synchronisation state will be TooFarAhead.
            // In this state, all players should be muted.
            assertMuted(PLAYER_1_ID, true);
            assertMuted(PLAYER_2_ID, true);

            // Send frames for both players, with more frames for player 2.
            sendFrames(PLAYER_1_ID, 5);
            sendFrames(PLAYER_2_ID, 20);

            // While both players are running, one of them should be un-muted.
            waitUntilRunning(PLAYER_1_ID);
            assertOnePlayerNotMuted();

            // After player 1 runs out of frames, the un-muted player should always be player 2.
            waitUntilPaused(PLAYER_1_ID);
            waitUntilRunning(PLAYER_2_ID);
            assertMuted(PLAYER_1_ID, true);
            assertMuted(PLAYER_2_ID, false);

            sendFrames(PLAYER_1_ID, 100);
            waitForCatchup(PLAYER_1_ID);
            waitUntilPaused(PLAYER_2_ID);
            assertMuted(PLAYER_1_ID, false);
            assertMuted(PLAYER_2_ID, true);

            sendFrames(PLAYER_2_ID, 100);
            waitForCatchup(PLAYER_2_ID);
            assertMuted(PLAYER_1_ID, false);
            assertMuted(PLAYER_2_ID, true);
        }

        [Test]
        public void TestSpectatingDuringGameplay()
        {
            int[] players = { PLAYER_1_ID, PLAYER_2_ID };

            start(players);
            sendFrames(players, 300);

            loadSpectateScreen();
            sendFrames(players, 300);

            AddUntilStep("playing from correct point in time", () => this.ChildrenOfType<DrawableRuleset>().All(r => r.FrameStableClock.CurrentTime > 30000));
        }

        [Test]
        public void TestSpectatingDuringGameplayWithLateFrames()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            sendFrames(new[] { PLAYER_1_ID, PLAYER_2_ID }, 300);

            loadSpectateScreen();
            sendFrames(PLAYER_1_ID, 300);

            AddWaitStep("wait maximum start delay seconds", (int)(SpectatorSyncManager.MAXIMUM_START_DELAY / TimePerAction));
            waitUntilRunning(PLAYER_1_ID);

            sendFrames(PLAYER_2_ID, 300);
            AddUntilStep("player 2 playing from correct point in time", () => getPlayer(PLAYER_2_ID).ChildrenOfType<DrawableRuleset>().Single().FrameStableClock.CurrentTime > 30000);
        }

        [Test]
        public void TestGameplayRateAdjust()
        {
            start(getPlayerIds(4), mods: new[] { new APIMod(new OsuModDoubleTime()) });

            loadSpectateScreen();

            sendFrames(getPlayerIds(4), 300);

            AddUntilStep("wait for correct track speed", () => Beatmap.Value.Track.Rate, () => Is.EqualTo(1.5));
        }

        [Test]
        public void TestPlayersLeaveWhileSpectating()
        {
            start(getPlayerIds(4));
            sendFrames(getPlayerIds(4), 300);

            loadSpectateScreen();

            for (int count = 3; count >= 0; count--)
            {
                int id = PLAYER_1_ID + count;

                end(id);
                AddUntilStep($"{id} area grayed", () => getInstance(id).Colour != Color4.White);
                AddUntilStep($"{id} score quit set", () => getLeaderboardScore(id).HasQuit.Value);
                sendFrames(getPlayerIds(count), 300);
            }

            Player? player = null;

            AddStep($"get {PLAYER_1_ID} player instance", () => player = getInstance(PLAYER_1_ID).ChildrenOfType<Player>().Single());

            start(new[] { PLAYER_1_ID });
            sendFrames(PLAYER_1_ID, 300);

            AddAssert($"{PLAYER_1_ID} player instance still same", () => getInstance(PLAYER_1_ID).ChildrenOfType<Player>().Single() == player);
            AddAssert($"{PLAYER_1_ID} area still grayed", () => getInstance(PLAYER_1_ID).Colour != Color4.White);
            AddAssert($"{PLAYER_1_ID} score quit still set", () => getLeaderboardScore(PLAYER_1_ID).HasQuit.Value);
        }

        /// <summary>
        /// Tests spectating with a beatmap that has a high <see cref="BeatmapInfo.AudioLeadIn"/> value.
        ///
        /// This test is not intended not to check the correct initial time value, but only to guard against
        /// gameplay potentially getting stuck in a stopped state due to lead in time being present.
        /// </summary>
        [Test]
        public void TestAudioLeadIn() => testLeadIn(b => b.BeatmapInfo.AudioLeadIn = 2000);

        /// <summary>
        /// Tests spectating with a beatmap that has a storyboard element with a negative start time (i.e. intro storyboard element).
        ///
        /// This test is not intended not to check the correct initial time value, but only to guard against
        /// gameplay potentially getting stuck in a stopped state due to lead in time being present.
        /// </summary>
        [Test]
        public void TestIntroStoryboardElement() => testLeadIn(b =>
        {
            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);
            sprite.TimelineGroup.Alpha.Add(Easing.None, -2000, 0, 0, 1);
            b.Storyboard.GetLayer("Background").Add(sprite);
        });

        private void testLeadIn(Action<WorkingBeatmap>? applyToBeatmap = null)
        {
            start(PLAYER_1_ID);

            loadSpectateScreen(false, applyToBeatmap);

            // to ensure negative gameplay start time does not affect spectator, send frames exactly after StartGameplay().
            // (similar to real spectating sessions in which the first frames get sent between StartGameplay() and player load complete)
            AddStep("send frames at gameplay start", () => getInstance(PLAYER_1_ID).OnGameplayStarted += () => SpectatorClient.SendFramesFromUser(PLAYER_1_ID, 100));

            AddUntilStep("wait for player load", () => spectatorScreen.AllPlayersLoaded);

            AddUntilStep("wait for clock running", () => getInstance(PLAYER_1_ID).SpectatorPlayerClock.IsRunning);

            assertNotCatchingUp(PLAYER_1_ID);
            waitUntilRunning(PLAYER_1_ID);
        }

        private void loadSpectateScreen(bool waitForPlayerLoad = true, Action<WorkingBeatmap>? applyToBeatmap = null)
        {
            AddStep("load screen", () =>
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(importedBeatmap);
                Ruleset.Value = importedBeatmap.Ruleset;

                applyToBeatmap?.Invoke(Beatmap.Value);

                LoadScreen(spectatorScreen = new MultiSpectatorScreen(SelectedRoom.Value, playingUsers.ToArray()));
            });

            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded && (!waitForPlayerLoad || spectatorScreen.AllPlayersLoaded));
        }

        private void start(int userId, int? beatmapId = null) => start(new[] { userId }, beatmapId);

        private void start(int[] userIds, int? beatmapId = null, APIMod[]? mods = null, bool teams = false)
        {
            AddStep("start play", () =>
            {
                for (int i = 0; i < userIds.Length; i++)
                {
                    int id = userIds[i];
                    var user = new MultiplayerRoomUser(id)
                    {
                        User = new APIUser { Id = id },
                        Mods = mods ?? Array.Empty<APIMod>(),
                        MatchState = teams ? new TeamVersusUserState { TeamID = i % 2 } : null,
                    };

                    OnlinePlayDependencies.MultiplayerClient.AddUser(user, true);
                    SpectatorClient.SendStartPlay(id, beatmapId ?? importedBeatmapId, mods);

                    playingUsers.Add(user);
                }
            });
        }

        private void end(int userId)
        {
            AddStep($"end play for {userId}", () =>
            {
                var user = playingUsers.Single(u => u.UserID == userId);

                SpectatorClient.SendEndPlay(userId);
                OnlinePlayDependencies.MultiplayerClient.RemoveUser(user.User.AsNonNull());

                playingUsers.Remove(user);
            });
        }

        /// <summary>
        /// Send new frames on behalf of a user.
        /// Frames will last for count * 100 milliseconds.
        /// </summary>
        private void sendFrames(int userId, int count = 10) => sendFrames(new[] { userId }, count);

        private void sendFrames(int[] userIds, int count = 10)
        {
            AddStep("send frames", () =>
            {
                foreach (int id in userIds)
                    SpectatorClient.SendFramesFromUser(id, count);
            });
        }

        private void checkRunningInstant(int userId)
        {
            waitUntilRunning(userId);

            // Todo: The following should work, but is broken because SpectatorScreen retrieves the WorkingBeatmap via the BeatmapManager, bypassing the test scene clock and running real-time.
            // AddAssert($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);
        }

        private void checkPausedInstant(int userId)
        {
            waitUntilPaused(userId);

            // Todo: The following should work, but is broken because SpectatorScreen retrieves the WorkingBeatmap via the BeatmapManager, bypassing the test scene clock and running real-time.
            // AddAssert($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);
        }

        private void assertOnePlayerNotMuted() => AddAssert(nameof(assertOnePlayerNotMuted), () => spectatorScreen.ChildrenOfType<PlayerArea>().Count(p => !p.Mute) == 1);

        private void assertMuted(int userId, bool muted)
            => AddAssert($"{nameof(assertMuted)}({userId}, {muted})", () => getInstance(userId).Mute == muted);

        private void assertRunning(int userId)
            => AddAssert($"{nameof(assertRunning)}({userId})", () => getInstance(userId).SpectatorPlayerClock.IsRunning);

        private void waitUntilPaused(int userId)
            => AddUntilStep($"{nameof(waitUntilPaused)}({userId})", () => !getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().IsRunning);

        private void waitUntilRunning(int userId)
            => AddUntilStep($"{nameof(waitUntilRunning)}({userId})", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().IsRunning);

        private void assertNotCatchingUp(int userId)
            => AddAssert($"{nameof(assertNotCatchingUp)}({userId})", () => !getInstance(userId).SpectatorPlayerClock.IsCatchingUp);

        private void waitForCatchup(int userId)
            => AddUntilStep($"{nameof(waitForCatchup)}({userId})", () => !getInstance(userId).SpectatorPlayerClock.IsCatchingUp);

        private Player getPlayer(int userId) => getInstance(userId).ChildrenOfType<Player>().Single();

        private PlayerArea getInstance(int userId) => spectatorScreen.ChildrenOfType<PlayerArea>().Single(p => p.UserId == userId);

        private GameplayLeaderboardScore getLeaderboardScore(int userId) => spectatorScreen.ChildrenOfType<GameplayLeaderboardScore>().Single(s => s.User?.OnlineID == userId);

        private int[] getPlayerIds(int count) => Enumerable.Range(PLAYER_1_ID, count).ToArray();
    }
}
