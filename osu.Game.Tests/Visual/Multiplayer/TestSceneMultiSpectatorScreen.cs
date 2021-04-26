// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiSpectatorScreen : MultiplayerTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient streamingClient = new TestSpectatorStreamingClient();

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private MultiSpectatorScreen spectatorScreen;

        private readonly List<int> playingUserIds = new List<int>();
        private readonly Dictionary<int, int> nextFrame = new Dictionary<int, int>();

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

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset sent frames", () => nextFrame.Clear());

            AddStep("add streaming client", () =>
            {
                Remove(streamingClient);
                Add(streamingClient);
            });

            AddStep("finish previous gameplay", () =>
            {
                foreach (var id in playingUserIds)
                    streamingClient.EndPlay(id, importedBeatmapId);
                playingUserIds.Clear();
            });
        }

        [Test]
        public void TestDelayedStart()
        {
            AddStep("start players silently", () =>
            {
                Client.CurrentMatchPlayingUserIds.Add(55);
                Client.CurrentMatchPlayingUserIds.Add(56);
                playingUserIds.Add(55);
                playingUserIds.Add(56);
                nextFrame[55] = 0;
                nextFrame[56] = 0;
            });

            loadSpectateScreen(false);

            AddWaitStep("wait a bit", 10);
            AddStep("load player 55", () => streamingClient.StartPlay(55, importedBeatmapId));
            AddUntilStep("one player added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 1);

            AddWaitStep("wait a bit", 10);
            AddStep("load player 56", () => streamingClient.StartPlay(56, importedBeatmapId));
            AddUntilStep("two players added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 2);
        }

        [Test]
        public void TestGeneral()
        {
            int[] userIds = Enumerable.Range(0, 4).Select(i => 55 + i).ToArray();

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestPlayersMustStartSimultaneously()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(55, 20);
            checkPausedInstant(55, true);
            checkPausedInstant(56, true);

            // Send frames for the other player, both should now start playing.
            sendFrames(56, 20);
            checkPausedInstant(55, false);
            checkPausedInstant(56, false);
        }

        [Test]
        public void TestPlayersDoNotStartSimultaneouslyIfBufferingForMaximumStartDelay()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(55, 1000);
            checkPausedInstant(55, true);
            checkPausedInstant(56, true);

            // Wait for the start delay seconds...
            AddWaitStep("wait maximum start delay seconds", (int)(CatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));

            // Player 1 should start playing by itself, player 2 should remain paused.
            checkPausedInstant(55, false);
            checkPausedInstant(56, true);
        }

        [Test]
        public void TestPlayersContinueWhileOthersBuffer()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(55, 20);
            sendFrames(56, 10);
            checkPausedInstant(55, false);
            checkPausedInstant(56, false);

            // Eventually player 2 will pause, player 1 must remain running.
            checkPaused(56, true);
            checkPausedInstant(55, false);

            // Eventually both players will run out of frames and should pause.
            checkPaused(55, true);
            checkPausedInstant(56, true);

            // Send more frames for the first player only. Player 1 should start playing with player 2 remaining paused.
            sendFrames(55, 20);
            checkPausedInstant(56, true);
            checkPausedInstant(55, false);

            // Send more frames for the second player. Both should be playing
            sendFrames(56, 20);
            checkPausedInstant(56, false);
            checkPausedInstant(55, false);
        }

        [Test]
        public void TestPlayersCatchUpAfterFallingBehind()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(55, 1000);
            sendFrames(56, 10);
            checkPausedInstant(55, false);
            checkPausedInstant(56, false);

            // Eventually player 2 will run out of frames and should pause.
            checkPaused(56, true);
            AddWaitStep("wait a few more frames", 10);

            // Send more frames for player 2. It should unpause.
            sendFrames(56, 1000);
            checkPausedInstant(56, false);

            // Player 2 should catch up to player 1 after unpausing.
            waitForCatchup(56);
            AddWaitStep("wait a bit", 10);
        }

        [Test]
        public void TestMostInSyncUserIsAudioSource()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            assertVolume(55, 0);
            assertVolume(56, 0);

            sendFrames(55, 10);
            sendFrames(56, 20);
            assertVolume(55, 1);
            assertVolume(56, 0);

            checkPaused(55, true);
            assertVolume(55, 0);
            assertVolume(56, 1);

            sendFrames(55, 100);
            waitForCatchup(55);
            checkPaused(56, true);
            assertVolume(55, 1);
            assertVolume(56, 0);

            sendFrames(56, 100);
            waitForCatchup(56);
            assertVolume(55, 1);
            assertVolume(56, 0);
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
                    streamingClient.StartPlay(id, beatmapId ?? importedBeatmapId);
                    playingUserIds.Add(id);
                    nextFrame[id] = 0;
                }
            });
        }

        private void finish(int userId, int? beatmapId = null)
        {
            AddStep("end play", () =>
            {
                streamingClient.EndPlay(userId, beatmapId ?? importedBeatmapId);
                playingUserIds.Remove(userId);
                nextFrame.Remove(userId);
            });
        }

        private void sendFrames(int userId, int count = 10) => sendFrames(new[] { userId }, count);

        private void sendFrames(int[] userIds, int count = 10)
        {
            AddStep("send frames", () =>
            {
                foreach (int id in userIds)
                {
                    streamingClient.SendFrames(id, nextFrame[id], count);
                    nextFrame[id] += count;
                }
            });
        }

        private void checkPaused(int userId, bool state)
            => AddUntilStep($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);

        private void checkPausedInstant(int userId, bool state)
            => AddAssert($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().GameplayClock.IsRunning != state);

        private void assertVolume(int userId, double volume)
            => AddAssert($"{userId} volume is {volume}", () => getInstance(userId).Volume.Value == volume);

        private void waitForCatchup(int userId)
            => AddUntilStep($"{userId} not catching up", () => !getInstance(userId).GameplayClock.IsCatchingUp);

        private Player getPlayer(int userId) => getInstance(userId).ChildrenOfType<Player>().Single();

        private PlayerArea getInstance(int userId) => spectatorScreen.ChildrenOfType<PlayerArea>().Single(p => p.UserId == userId);

        public class TestSpectatorStreamingClient : SpectatorStreamingClient
        {
            private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
            private readonly Dictionary<int, bool> userSentStateDictionary = new Dictionary<int, bool>();

            public TestSpectatorStreamingClient()
                : base(new DevelopmentEndpointConfiguration())
            {
            }

            public void StartPlay(int userId, int beatmapId)
            {
                userBeatmapDictionary[userId] = beatmapId;
                userSentStateDictionary[userId] = false;

                sendState(userId, beatmapId);
            }

            public void EndPlay(int userId, int beatmapId)
            {
                ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });

                userSentStateDictionary[userId] = false;
            }

            public void SendFrames(int userId, int index, int count)
            {
                var frames = new List<LegacyReplayFrame>();

                for (int i = index; i < index + count; i++)
                {
                    var buttonState = i == index + count - 1 ? ReplayButtonState.None : ReplayButtonState.Left1;

                    frames.Add(new LegacyReplayFrame(i * 100, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
                }

                var bundle = new FrameDataBundle(new ScoreInfo { Combo = index + count }, frames);
                ((ISpectatorClient)this).UserSentFrames(userId, bundle);

                if (!userSentStateDictionary[userId])
                    sendState(userId, userBeatmapDictionary[userId]);
            }

            public override void WatchUser(int userId)
            {
                if (!PlayingUsers.Contains(userId) && userSentStateDictionary.TryGetValue(userId, out var sent) && sent)
                {
                    // usually the server would do this.
                    sendState(userId, userBeatmapDictionary[userId]);
                }

                base.WatchUser(userId);
            }

            private void sendState(int userId, int beatmapId)
            {
                ((ISpectatorClient)this).UserBeganPlaying(userId, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });

                userSentStateDictionary[userId] = true;
            }
        }

        internal class TestUserLookupCache : UserLookupCache
        {
            protected override Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
            {
                return Task.FromResult(new User
                {
                    Id = lookup,
                    Username = $"User {lookup}"
                });
            }
        }
    }
}
