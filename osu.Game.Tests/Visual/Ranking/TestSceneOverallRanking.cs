// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics.User;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneOverallRanking : OsuTestScene
    {
        private OverallRanking overallRanking = null!;

        [Test]
        public void TestUpdatePending()
        {
            createDisplay();
        }

        [Test]
        public void TestAllIncreased()
        {
            createDisplay();
            displayUpdate(
                new UserStatistics
                {
                    GlobalRank = 12_345,
                    Accuracy = 98.99,
                    MaxCombo = 2_322,
                    RankedScore = 23_123_543_456,
                    TotalScore = 123_123_543_456,
                    PP = 5_072
                },
                new UserStatistics
                {
                    GlobalRank = 1_234,
                    Accuracy = 99.07,
                    MaxCombo = 2_352,
                    RankedScore = 23_124_231_435,
                    TotalScore = 123_124_231_435,
                    PP = 5_434
                });
        }

        [Test]
        public void TestAllDecreased()
        {
            createDisplay();
            displayUpdate(
                new UserStatistics
                {
                    GlobalRank = 1_234,
                    Accuracy = 99.07,
                    MaxCombo = 2_352,
                    RankedScore = 23_124_231_435,
                    TotalScore = 123_124_231_435,
                    PP = 5_434
                },
                new UserStatistics
                {
                    GlobalRank = 12_345,
                    Accuracy = 98.99,
                    MaxCombo = 2_322,
                    RankedScore = 23_123_543_456,
                    TotalScore = 123_123_543_456,
                    PP = 5_072
                });
        }

        [Test]
        public void TestNoChanges()
        {
            var statistics = new UserStatistics
            {
                GlobalRank = 12_345,
                Accuracy = 98.99,
                MaxCombo = 2_322,
                RankedScore = 23_123_543_456,
                TotalScore = 123_123_543_456,
                PP = 5_072
            };

            createDisplay();
            displayUpdate(statistics, statistics);
        }

        [Test]
        public void TestNotRanked()
        {
            var statistics = new UserStatistics
            {
                GlobalRank = null,
                Accuracy = 98.99,
                MaxCombo = 2_322,
                RankedScore = 23_123_543_456,
                TotalScore = 123_123_543_456,
                PP = null
            };

            createDisplay();
            displayUpdate(statistics, statistics);
        }

        private void createDisplay() => AddStep("create display", () => Child = overallRanking = new OverallRanking
        {
            Width = 400,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        private void displayUpdate(UserStatistics before, UserStatistics after) =>
            AddStep("display update", () => overallRanking.StatisticsUpdate.Value = new UserStatisticsUpdate(new ScoreInfo(), before, after));
    }
}
