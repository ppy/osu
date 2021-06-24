// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.Spectator;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiSpectatorLeaderboard : OsuTestScene
    {
        private Dictionary<int, ManualClock> clocks;

        private MultiSpectatorLeaderboard leaderboard;
        private TestContainer container;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset", () =>
            {
                clocks = new Dictionary<int, ManualClock>
                {
                    { MultiplayerTestScene.PLAYER_1_ID, new ManualClock() },
                    { MultiplayerTestScene.PLAYER_2_ID, new ManualClock() }
                };

                Child = container = new TestContainer();

                foreach (var (userId, _) in clocks)
                    container.SpectatorClient.StartPlay(userId, 0);
            });

            AddStep("create leaderboard", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
                var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
                var scoreProcessor = new OsuScoreProcessor();
                scoreProcessor.ApplyBeatmap(playable);

                container.LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(scoreProcessor, clocks.Keys.ToArray()) { Expanded = { Value = true } }, container.Add);
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
                // For player 1, send frames in sets of 1.
                // For player 2, send frames in sets of 10.
                for (int i = 0; i < 100; i++)
                {
                    container.SpectatorClient.SendFrames(MultiplayerTestScene.PLAYER_1_ID, i, 1);

                    if (i % 10 == 0)
                        container.SpectatorClient.SendFrames(MultiplayerTestScene.PLAYER_2_ID, i, 10);
                }
            });

            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 1);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 10);

            // Advance to a point where only user player 1's frame changes.
            setTime(500);
            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 5);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 10);

            // Advance to a point where both user's frame changes.
            setTime(1100);
            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 11);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 20);

            // Advance user player 2 only to a point where its frame changes.
            setTime(MultiplayerTestScene.PLAYER_2_ID, 2100);
            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 11);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 30);

            // Advance both users beyond their last frame
            setTime(101 * 100);
            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 100);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 100);
        }

        [Test]
        public void TestNoFrames()
        {
            assertCombo(MultiplayerTestScene.PLAYER_1_ID, 0);
            assertCombo(MultiplayerTestScene.PLAYER_2_ID, 0);
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

        private class TestContainer : TestMultiplayerRoomContainer
        {
            [Cached(typeof(SpectatorClient))]
            public readonly TestSpectatorClient SpectatorClient = new TestSpectatorClient();

            [Cached(typeof(UserLookupCache))]
            public readonly UserLookupCache LookupCache = new TestUserLookupCache();

            protected override Container<Drawable> Content => content;
            private readonly Container content;

            public TestContainer()
            {
                AddRangeInternal(new Drawable[]
                {
                    SpectatorClient,
                    LookupCache,
                    content = new Container { RelativeSizeAxes = Axes.Both }
                });
            }

            public new Task LoadComponentAsync<TLoadable>([NotNull] TLoadable component, Action<TLoadable> onLoaded = null, CancellationToken cancellation = default, Scheduler scheduler = null)
                where TLoadable : Drawable
                => base.LoadComponentAsync(component, onLoaded, cancellation, scheduler);
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
