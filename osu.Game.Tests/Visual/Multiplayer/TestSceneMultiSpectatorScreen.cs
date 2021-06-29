// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiSpectatorScreen : MultiplayerTestScene
    {
        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private MultiSpectatorScreen spectatorScreen;

        private readonly List<int> playingUserIds = new List<int>();

        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;
        private int importedBeatmapId;

        [BackgroundDependencyLoader]
        private void load()
        {
            importedSet = ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Result;
            importedBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
            importedBeatmapId = importedBeatmap.OnlineBeatmapID ?? -1;
        }

        [SetUp]
        public new void Setup() => Schedule(() => playingUserIds.Clear());

        [Test]
        public void TestDelayedStart()
        {
            AddStep("start players silently", () =>
            {
                Client.CurrentMatchPlayingUserIds.Add(PLAYER_1_ID);
                Client.CurrentMatchPlayingUserIds.Add(PLAYER_2_ID);
                playingUserIds.Add(PLAYER_1_ID);
                playingUserIds.Add(PLAYER_2_ID);
            });

            loadSpectateScreen(false);

            AddWaitStep("wait a bit", 10);
            AddStep("load player first_player_id", () => SpectatorClient.StartPlay(PLAYER_1_ID, importedBeatmapId));
            AddUntilStep("one player added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 1);

            AddWaitStep("wait a bit", 10);
            AddStep("load player second_player_id", () => SpectatorClient.StartPlay(PLAYER_2_ID, importedBeatmapId));
            AddUntilStep("two players added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 2);
        }

        [Test]
        public void TestGeneral()
        {
            int[] userIds = Enumerable.Range(0, 4).Select(i => PLAYER_1_ID + i).ToArray();

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestTimeDoesNotProgressWhileAllPlayersPaused()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            sendFrames(PLAYER_1_ID, 20);
            sendFrames(PLAYER_2_ID, 10);

            checkPaused(PLAYER_2_ID, true);
            checkPausedInstant(PLAYER_1_ID, false);
            AddAssert("master clock still running", () => this.ChildrenOfType<MasterGameplayClockContainer>().Single().IsRunning);

            checkPaused(PLAYER_1_ID, true);
            AddUntilStep("master clock paused", () => !this.ChildrenOfType<MasterGameplayClockContainer>().Single().IsRunning);
        }

        [Test]
        public void TestPlayersMustStartSimultaneously()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(PLAYER_1_ID, 20);
            checkPausedInstant(PLAYER_1_ID, true);
            checkPausedInstant(PLAYER_2_ID, true);

            // Send frames for the other player, both should now start playing.
            sendFrames(PLAYER_2_ID, 20);
            checkPausedInstant(PLAYER_1_ID, false);
            checkPausedInstant(PLAYER_2_ID, false);
        }

        [Test]
        public void TestPlayersDoNotStartSimultaneouslyIfBufferingForMaximumStartDelay()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(PLAYER_1_ID, 1000);
            checkPausedInstant(PLAYER_1_ID, true);
            checkPausedInstant(PLAYER_2_ID, true);

            // Wait for the start delay seconds...
            AddWaitStep("wait maximum start delay seconds", (int)(CatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));

            // Player 1 should start playing by itself, player 2 should remain paused.
            checkPausedInstant(PLAYER_1_ID, false);
            checkPausedInstant(PLAYER_2_ID, true);
        }

        [Test]
        public void TestPlayersContinueWhileOthersBuffer()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(PLAYER_1_ID, 20);
            sendFrames(PLAYER_2_ID, 10);
            checkPausedInstant(PLAYER_1_ID, false);
            checkPausedInstant(PLAYER_2_ID, false);

            // Eventually player 2 will pause, player 1 must remain running.
            checkPaused(PLAYER_2_ID, true);
            checkPausedInstant(PLAYER_1_ID, false);

            // Eventually both players will run out of frames and should pause.
            checkPaused(PLAYER_1_ID, true);
            checkPausedInstant(PLAYER_2_ID, true);

            // Send more frames for the first player only. Player 1 should start playing with player 2 remaining paused.
            sendFrames(PLAYER_1_ID, 20);
            checkPausedInstant(PLAYER_2_ID, true);
            checkPausedInstant(PLAYER_1_ID, false);

            // Send more frames for the second player. Both should be playing
            sendFrames(PLAYER_2_ID, 20);
            checkPausedInstant(PLAYER_2_ID, false);
            checkPausedInstant(PLAYER_1_ID, false);
        }

        [Test]
        public void TestPlayersCatchUpAfterFallingBehind()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(PLAYER_1_ID, 1000);
            sendFrames(PLAYER_2_ID, 10);
            checkPausedInstant(PLAYER_1_ID, false);
            checkPausedInstant(PLAYER_2_ID, false);

            // Eventually player 2 will run out of frames and should pause.
            checkPaused(PLAYER_2_ID, true);
            AddWaitStep("wait a few more frames", 10);

            // Send more frames for player 2. It should unpause.
            sendFrames(PLAYER_2_ID, 1000);
            checkPausedInstant(PLAYER_2_ID, false);

            // Player 2 should catch up to player 1 after unpausing.
            waitForCatchup(PLAYER_2_ID);
            AddWaitStep("wait a bit", 10);
        }

        [Test]
        public void TestMostInSyncUserIsAudioSource()
        {
            start(new[] { PLAYER_1_ID, PLAYER_2_ID });
            loadSpectateScreen();

            assertMuted(PLAYER_1_ID, true);
            assertMuted(PLAYER_2_ID, true);

            sendFrames(PLAYER_1_ID, 10);
            sendFrames(PLAYER_2_ID, 20);
            assertMuted(PLAYER_1_ID, false);
            assertMuted(PLAYER_2_ID, true);

            checkPaused(PLAYER_1_ID, true);
            assertMuted(PLAYER_1_ID, true);
            assertMuted(PLAYER_2_ID, false);

            sendFrames(PLAYER_1_ID, 100);
            waitForCatchup(PLAYER_1_ID);
            checkPaused(PLAYER_2_ID, true);
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
            var players = new[] { PLAYER_1_ID, PLAYER_2_ID };

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

            AddWaitStep("wait maximum start delay seconds", (int)(CatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));
            checkPaused(PLAYER_1_ID, false);

            sendFrames(PLAYER_2_ID, 300);
            AddUntilStep("player 2 playing from correct point in time", () => getPlayer(PLAYER_2_ID).ChildrenOfType<DrawableRuleset>().Single().FrameStableClock.CurrentTime > 30000);
        }

        private void loadSpectateScreen(bool waitForPlayerLoad = true)
        {
            AddStep("load screen", () =>
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(importedBeatmap);
                Ruleset.Value = importedBeatmap.Ruleset;

                LoadScreen(spectatorScreen = new MultiSpectatorScreen(playingUserIds.ToArray()));
            });

            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded && (!waitForPlayerLoad || spectatorScreen.AllPlayersLoaded));
        }

        private void start(int userId, int? beatmapId = null) => start(new[] { userId }, beatmapId);

        private void start(int[] userIds, int? beatmapId = null)
        {
            AddStep("start play", () =>
            {
                foreach (int id in userIds)
                {
                    Client.CurrentMatchPlayingUserIds.Add(id);
                    SpectatorClient.StartPlay(id, beatmapId ?? importedBeatmapId);
                    playingUserIds.Add(id);
                }
            });
        }

        private void finish(int userId)
        {
            AddStep("end play", () =>
            {
                SpectatorClient.EndPlay(userId);
                playingUserIds.Remove(userId);
            });
        }

        private void sendFrames(int userId, int count = 10) => sendFrames(new[] { userId }, count);

        private void sendFrames(int[] userIds, int count = 10)
        {
            AddStep("send frames", () =>
            {
                foreach (int id in userIds)
                    SpectatorClient.SendFrames(id, count);
            });
        }

        private void checkPaused(int userId, bool state)
            => AddUntilStep($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);

        private void checkPausedInstant(int userId, bool state)
            => AddAssert($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);

        private void assertMuted(int userId, bool muted)
            => AddAssert($"{userId} {(muted ? "is" : "is not")} muted", () => getInstance(userId).Mute == muted);

        private void waitForCatchup(int userId)
            => AddUntilStep($"{userId} not catching up", () => !getInstance(userId).GameplayClock.IsCatchingUp);

        private Player getPlayer(int userId) => getInstance(userId).ChildrenOfType<Player>().Single();

        private PlayerArea getInstance(int userId) => spectatorScreen.ChildrenOfType<PlayerArea>().Single(p => p.UserId == userId);
    }
}
