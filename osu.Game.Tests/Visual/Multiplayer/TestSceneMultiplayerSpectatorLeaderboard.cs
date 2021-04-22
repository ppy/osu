// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerSpectatorLeaderboard : MultiplayerTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient streamingClient = new TestSpectatorStreamingClient();

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        private readonly Dictionary<int, ManualClock> clocks = new Dictionary<int, ManualClock>
        {
            { 55, new ManualClock() },
            { 56, new ManualClock() }
        };

        public TestSceneMultiplayerSpectatorLeaderboard()
        {
            base.Content.AddRange(new Drawable[]
            {
                streamingClient,
                lookupCache,
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }

        [SetUpSteps]
        public new void SetUpSteps()
        {
            MultiSpectatorLeaderboard leaderboard = null;

            AddStep("reset", () =>
            {
                Clear();

                foreach (var (userId, clock) in clocks)
                {
                    streamingClient.EndPlay(userId, 0);
                    clock.CurrentTime = 0;
                }
            });

            AddStep("create leaderboard", () =>
            {
                foreach (var (userId, _) in clocks)
                    streamingClient.StartPlay(userId, 0);

                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
                var scoreProcessor = new OsuScoreProcessor();
                scoreProcessor.ApplyBeatmap(playable);

                LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(scoreProcessor, clocks.Keys.ToArray()) { Expanded = { Value = true } }, Add);
            });

            AddUntilStep("wait for load", () => leaderboard.IsLoaded);

            AddStep("add clock sources", () =>
            {
                foreach (var (userId, clock) in clocks)
                    leaderboard.AddClock(userId, clock);
            });
        }

        [Test]
        public void TestLeaderboardTracksCurrentTime()
        {
            AddStep("send frames", () =>
            {
                // For user 55, send frames in sets of 1.
                // For user 56, send frames in sets of 10.
                for (int i = 0; i < 100; i++)
                {
                    streamingClient.SendFrames(55, i, 1);

                    if (i % 10 == 0)
                        streamingClient.SendFrames(56, i, 10);
                }
            });

            assertCombo(55, 1);
            assertCombo(56, 10);

            // Advance to a point where only user 55's frame changes.
            setTime(500);
            assertCombo(55, 5);
            assertCombo(56, 10);

            // Advance to a point where both user's frame changes.
            setTime(1100);
            assertCombo(55, 11);
            assertCombo(56, 20);

            // Advance user 56 only to a point where its frame changes.
            setTime(56, 2100);
            assertCombo(55, 11);
            assertCombo(56, 30);

            // Advance both users beyond their last frame
            setTime(101 * 100);
            assertCombo(55, 100);
            assertCombo(56, 100);
        }

        [Test]
        public void TestNoFrames()
        {
            assertCombo(55, 0);
            assertCombo(56, 0);
        }

        private void setTime(double time) => AddStep($"set time {time}", () =>
        {
            foreach (var (_, clock) in clocks)
                clock.CurrentTime = time;
        });

        private void setTime(int userId, double time)
            => AddStep($"set user {userId} time {time}", () => clocks[userId].CurrentTime = time);

        private void assertCombo(int userId, int expectedCombo)
            => AddUntilStep($"player {userId} has {expectedCombo} combo", () => this.ChildrenOfType<GameplayLeaderboardScore>().Single(s => s.User?.Id == userId).Combo.Value == expectedCombo);

        private class TestSpectatorStreamingClient : SpectatorStreamingClient
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

        private class TestUserLookupCache : UserLookupCache
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
