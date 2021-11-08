// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiSpectatorLeaderboard : MultiplayerTestScene
    {
        private Dictionary<int, ManualClock> clocks;
        private MultiSpectatorLeaderboard leaderboard;

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("reset", () =>
            {
                Clear();

                clocks = new Dictionary<int, ManualClock>
                {
                    { PLAYER_1_ID, new ManualClock() },
                    { PLAYER_2_ID, new ManualClock() }
                };

                foreach ((int userId, var _) in clocks)
                {
                    SpectatorClient.StartPlay(userId, 0);
                    OnlinePlayDependencies.Client.AddUser(new APIUser { Id = userId });
                }
            });

            AddStep("create leaderboard", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
                var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
                var scoreProcessor = new OsuScoreProcessor();
                scoreProcessor.ApplyBeatmap(playable);

                LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(scoreProcessor, clocks.Keys.Select(id => new MultiplayerRoomUser(id)).ToArray()) { Expanded = { Value = true } }, Add);
            });

            AddUntilStep("wait for load", () => leaderboard.IsLoaded);

            AddStep("add clock sources", () =>
            {
                foreach ((int userId, var clock) in clocks)
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
                    SpectatorClient.SendFrames(PLAYER_1_ID, 1);

                    if (i % 10 == 0)
                        SpectatorClient.SendFrames(PLAYER_2_ID, 10);
                }
            });

            assertCombo(PLAYER_1_ID, 1);
            assertCombo(PLAYER_2_ID, 10);

            // Advance to a point where only user player 1's frame changes.
            setTime(500);
            assertCombo(PLAYER_1_ID, 5);
            assertCombo(PLAYER_2_ID, 10);

            // Advance to a point where both user's frame changes.
            setTime(1100);
            assertCombo(PLAYER_1_ID, 11);
            assertCombo(PLAYER_2_ID, 20);

            // Advance user player 2 only to a point where its frame changes.
            setTime(PLAYER_2_ID, 2100);
            assertCombo(PLAYER_1_ID, 11);
            assertCombo(PLAYER_2_ID, 30);

            // Advance both users beyond their last frame
            setTime(101 * 100);
            assertCombo(PLAYER_1_ID, 100);
            assertCombo(PLAYER_2_ID, 100);
        }

        [Test]
        public void TestNoFrames()
        {
            assertCombo(PLAYER_1_ID, 0);
            assertCombo(PLAYER_2_ID, 0);
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
    }
}
