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
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerSpectator : MultiplayerTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSpectatorStreamingClient();

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private MultiplayerSpectator spectator;

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
                Remove(testSpectatorStreamingClient);
                Add(testSpectatorStreamingClient);
            });

            AddStep("finish previous gameplay", () =>
            {
                foreach (var id in playingUserIds)
                    testSpectatorStreamingClient.EndPlay(id, importedBeatmapId);
                playingUserIds.Clear();
            });
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
        public void TestPlayersDoNotStartSimultaneouslyIfBufferingFor15Seconds()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send frames for one player only, both should remain paused.
            sendFrames(55, 20);
            checkPausedInstant(55, true);
            checkPausedInstant(56, true);

            // Wait 15 seconds...
            AddWaitStep("wait 15 seconds", (int)(15000 / TimePerAction));

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
            sendFrames(55, 100);
            sendFrames(56, 10);
            checkPausedInstant(55, false);
            checkPausedInstant(56, false);

            // Eventually player 2 will run out of frames and should pause.
            checkPaused(56, true);
            AddWaitStep("wait a few more frames", 10);

            // Send more frames for player 2. It should unpause.
            sendFrames(56, 100);
            checkPausedInstant(56, false);

            // Player 2 should catch up to player 1 after unpausing.
            AddUntilStep("player 1 time == player 2 time", () => Precision.AlmostEquals(getGameplayTime(55), getGameplayTime(56), 16));
        }

        private void loadSpectateScreen()
        {
            AddStep("load screen", () =>
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(importedBeatmap);
                Ruleset.Value = importedBeatmap.Ruleset;

                LoadScreen(spectator = new MultiplayerSpectator(playingUserIds.ToArray()));
            });

            AddUntilStep("wait for screen load", () => spectator.LoadState == LoadState.Loaded && spectator.AllPlayersLoaded);
        }

        private void start(int userId, int? beatmapId = null) => start(new[] { userId }, beatmapId);

        private void start(int[] userIds, int? beatmapId = null)
        {
            AddStep("start play", () =>
            {
                foreach (int id in userIds)
                {
                    Client.CurrentMatchPlayingUserIds.Add(id);
                    testSpectatorStreamingClient.StartPlay(id, beatmapId ?? importedBeatmapId);
                    playingUserIds.Add(id);
                    nextFrame[id] = 0;
                }
            });
        }

        private void finish(int userId, int? beatmapId = null)
        {
            AddStep("end play", () =>
            {
                testSpectatorStreamingClient.EndPlay(userId, beatmapId ?? importedBeatmapId);
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
                    testSpectatorStreamingClient.SendFrames(id, nextFrame[id], count);
                    nextFrame[id] += count;
                }
            });
        }

        private void checkPaused(int userId, bool state) =>
            AddUntilStep($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().IsPaused.Value == state);

        private void checkPausedInstant(int userId, bool state) =>
            AddAssert($"{userId} is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().First().IsPaused.Value == state);

        private double getGameplayTime(int userId) => getPlayer(userId).ChildrenOfType<GameplayClockContainer>().Single().GameplayClock.CurrentTime;

        private Player getPlayer(int userId) => getInstance(userId).ChildrenOfType<Player>().Single();

        private PlayerInstance getInstance(int userId) => spectator.ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == userId);

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
                if (userSentStateDictionary[userId])
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
