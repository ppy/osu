// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osu.Game.Users;
using OverallRanking = osu.Game.Screens.Ranking.Statistics.User.OverallRanking;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneOverallRanking : OsuTestScene
    {
        [Cached(typeof(ISoloStatisticsWatcher))]
        private MockSoloStatisticsWatcher soloStatisticsWatcher { get; } = new MockSoloStatisticsWatcher();

        [Test]
        public void TestUpdatePending()
        {
            createDisplay();
        }

        [Test]
        public void TestAllIncreased()
        {
            createDisplay();
            AddStep("trigger update success", () =>
            {
                soloStatisticsWatcher.TriggerSuccess(
                    new UserStatistics
                    {
                        GlobalRank = 12_345,
                        Accuracy = 0.9899,
                        MaxCombo = 2_322,
                        RankedScore = 23_123_543_456,
                        TotalScore = 123_123_543_456,
                        PP = 5_072
                    },
                    new UserStatistics
                    {
                        GlobalRank = 1_234,
                        Accuracy = 0.9907,
                        MaxCombo = 2_352,
                        RankedScore = 23_124_231_435,
                        TotalScore = 123_124_231_435,
                        PP = 5_434
                    });
            });
        }

        [Test]
        public void TestAllDecreased()
        {
            createDisplay();
            AddStep("trigger update success", () =>
            {
                soloStatisticsWatcher.TriggerSuccess(
                    new UserStatistics
                    {
                        GlobalRank = 1_234,
                        Accuracy = 0.9907,
                        MaxCombo = 2_352,
                        RankedScore = 23_124_231_435,
                        TotalScore = 123_124_231_435,
                        PP = 5_434
                    },
                    new UserStatistics
                    {
                        GlobalRank = 12_345,
                        Accuracy = 0.9899,
                        MaxCombo = 2_322,
                        RankedScore = 23_123_543_456,
                        TotalScore = 123_123_543_456,
                        PP = 5_072
                    });
            });
        }

        [Test]
        public void TestNoChanges()
        {
            var statistics = new UserStatistics
            {
                GlobalRank = 12_345,
                Accuracy = 0.9899,
                MaxCombo = 2_322,
                RankedScore = 23_123_543_456,
                TotalScore = 123_123_543_456,
                PP = 5_072
            };

            createDisplay();
            AddStep("trigger update success", () => soloStatisticsWatcher.TriggerSuccess(statistics, statistics));
        }

        [Test]
        public void TestNotRanked()
        {
            var statistics = new UserStatistics
            {
                GlobalRank = null,
                Accuracy = 0.9899,
                MaxCombo = 2_322,
                RankedScore = 23_123_543_456,
                TotalScore = 123_123_543_456,
                PP = null
            };

            createDisplay();
            AddStep("trigger update success", () => soloStatisticsWatcher.TriggerSuccess(statistics, statistics));
        }

        private void createDisplay() => AddStep("create display", () => Child = new OverallRanking(new ScoreInfo())
        {
            Width = 400,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        private class MockSoloStatisticsWatcher : ISoloStatisticsWatcher
        {
            private ScoreInfo? score;
            private Action<SoloStatisticsUpdate>? onUpdateReady;

            public void RegisterForStatisticsUpdateAfter(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady)
            {
                this.score = score;
                this.onUpdateReady = onUpdateReady;
            }

            public void TriggerSuccess(UserStatistics before, UserStatistics after)
            {
                Debug.Assert(score != null && onUpdateReady != null);
                onUpdateReady.Invoke(new SoloStatisticsUpdate(score, before, after));
            }
        }
    }
}
