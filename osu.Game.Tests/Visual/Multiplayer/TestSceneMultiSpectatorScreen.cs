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

        private const int player_1_id = 55;
        private const int player_2_id = 56;

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
                Client.CurrentMatchPlayingUserIds.Add(player_1_id);
                Client.CurrentMatchPlayingUserIds.Add(player_2_id);
                playingUserIds.Add(player_1_id);
                playingUserIds.Add(player_2_id);
                nextFrame[player_1_id] = 0;
                nextFrame[player_2_id] = 0;
            });

            loadSpectateScreen(false);

            AddWaitStep("wait a bit", 10);
            AddStep("load player first_player_id", () => streamingClient.StartPlay(player_1_id, importedBeatmapId));
            AddUntilStep("one player added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 1);

            AddWaitStep("wait a bit", 10);
            AddStep("load player second_player_id", () => streamingClient.StartPlay(player_2_id, importedBeatmapId));
            AddUntilStep("two players added", () => spectatorScreen.ChildrenOfType<Player>().Count() == 2);
        }

        [Test]
        public void TestGeneral()
        {
            int[] userIds = Enumerable.Range(0, 4).Select(i => player_1_id + i).ToArray();

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestPlayersMustStartSimultaneously()
        {
            start(new[] { player_1_id, player_2_id });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(player_1_id, 20);
            checkPausedInstant(player_1_id, true);
            checkPausedInstant(player_2_id, true);

            // Send frames for the other player, both should now start playing.
            sendFrames(player_2_id, 20);
            checkPausedInstant(player_1_id, false);
            checkPausedInstant(player_2_id, false);
        }

        [Test]
        public void TestPlayersDoNotStartSimultaneouslyIfBufferingForMaximumStartDelay()
        {
            start(new[] { player_1_id, player_2_id });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(player_1_id, 1000);
            checkPausedInstant(player_1_id, true);
            checkPausedInstant(player_2_id, true);

            // Wait for the start delay seconds...
            AddWaitStep("wait maximum start delay seconds", (int)(CatchUpSyncManager.MAXIMUM_START_DELAY / TimePerAction));

            // Player 1 should start playing by itself, player 2 should remain paused.
            checkPausedInstant(player_1_id, false);
            checkPausedInstant(player_2_id, true);
        }

        [Test]
        public void TestPlayersContinueWhileOthersBuffer()
        {
            start(new[] { player_1_id, player_2_id });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(player_1_id, 20);
            sendFrames(player_2_id, 10);
            checkPausedInstant(player_1_id, false);
            checkPausedInstant(player_2_id, false);

            // Eventually player 2 will pause, player 1 must remain running.
            checkPaused(player_2_id, true);
            checkPausedInstant(player_1_id, false);

            // Eventually both players will run out of frames and should pause.
            checkPaused(player_1_id, true);
            checkPausedInstant(player_2_id, true);

            // Send more frames for the first player only. Player 1 should start playing with player 2 remaining paused.
            sendFrames(player_1_id, 20);
            checkPausedInstant(player_2_id, true);
            checkPausedInstant(player_1_id, false);

            // Send more frames for the second player. Both should be playing
            sendFrames(player_2_id, 20);
            checkPausedInstant(player_2_id, false);
            checkPausedInstant(player_1_id, false);
        }

        [Test]
        public void TestPlayersCatchUpAfterFallingBehind()
        {
            start(new[] { player_1_id, player_2_id });
            loadSpectateScreen();

            // Send initial frames for both players. A few more for player 1.
            sendFrames(player_1_id, 1000);
            sendFrames(player_2_id, 10);
            checkPausedInstant(player_1_id, false);
            checkPausedInstant(player_2_id, false);

            // Eventually player 2 will run out of frames and should pause.
            checkPaused(player_2_id, true);
            AddWaitStep("wait a few more frames", 10);

            // Send more frames for player 2. It should unpause.
            sendFrames(player_2_id, 1000);
            checkPausedInstant(player_2_id, false);

            // Player 2 should catch up to player 1 after unpausing.
            waitForCatchup(player_2_id);
            AddWaitStep("wait a bit", 10);
        }

        [Test]
        public void TestMostInSyncUserIsAudioSource()
        {
            start(new[] { player_1_id, player_2_id });
            loadSpectateScreen();

            assertVolume(player_1_id, 0);
            assertVolume(player_2_id, 0);

            sendFrames(player_1_id, 10);
            sendFrames(player_2_id, 20);
            assertVolume(player_1_id, 1);
            assertVolume(player_2_id, 0);

            checkPaused(player_1_id, true);
            assertVolume(player_1_id, 0);
            assertVolume(player_2_id, 1);

            sendFrames(player_1_id, 100);
            waitForCatchup(player_1_id);
            checkPaused(player_2_id, true);
            assertVolume(player_1_id, 1);
            assertVolume(player_2_id, 0);

            sendFrames(player_2_id, 100);
            waitForCatchup(player_2_id);
            assertVolume(player_1_id, 1);
            assertVolume(player_2_id, 0);
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
