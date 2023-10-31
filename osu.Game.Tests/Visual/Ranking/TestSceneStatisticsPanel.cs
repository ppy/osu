// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneStatisticsPanel : OsuTestScene
    {
        [Test]
        public void TestScoreWithPositionStatistics()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.OnlineID = 1234;
            score.HitEvents = CreatePositionDistributedHitEvents();

            loadPanel(score);
        }

        [Test]
        public void TestScoreWithTimeStatistics()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents();

            loadPanel(score);
        }

        [Test]
        public void TestScoreWithoutStatistics()
        {
            loadPanel(TestResources.CreateTestScoreInfo());
        }

        [Test]
        public void TestScoreInRulesetWhereAllStatsRequireHitEvents()
        {
            loadPanel(TestResources.CreateTestScoreInfo(new TestRulesetAllStatsRequireHitEvents().RulesetInfo));
        }

        [Test]
        public void TestScoreInRulesetWhereNoStatsRequireHitEvents()
        {
            loadPanel(TestResources.CreateTestScoreInfo(new TestRulesetNoStatsRequireHitEvents().RulesetInfo));
        }

        [Test]
        public void TestScoreInMixedRuleset()
        {
            loadPanel(TestResources.CreateTestScoreInfo(new TestRulesetMixed().RulesetInfo));
        }

        [Test]
        public void TestNullScore()
        {
            loadPanel(null);
        }

        private void loadPanel(ScoreInfo score) => AddStep("load panel", () =>
        {
            Child = new SoloStatisticsPanel(score)
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                Score = { Value = score },
                StatisticsUpdate =
                {
                    Value = new SoloStatisticsUpdate(score, new UserStatistics
                    {
                        Level = new UserStatistics.LevelInfo
                        {
                            Current = 5,
                            Progress = 20,
                        },
                        GlobalRank = 38000,
                        CountryRank = 12006,
                        PP = 2134,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        PlayCount = 13375,
                        PlayTime = 354490,
                        TotalScore = 128749597,
                        TotalHits = 0,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        Level = new UserStatistics.LevelInfo
                        {
                            Current = 5,
                            Progress = 30,
                        },
                        GlobalRank = 36000,
                        CountryRank = 12000,
                        PP = (decimal)2134.5,
                        RankedScore = 23897015,
                        Accuracy = 0.984,
                        PlayCount = 13376,
                        PlayTime = 35789,
                        TotalScore = 132218497,
                        TotalHits = 0,
                        MaxCombo = 1233,
                    })
                }
            };
        });

        public static List<HitEvent> CreatePositionDistributedHitEvents()
        {
            var hitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents();

            // Use constant seed for reproducibility
            var random = new Random(0);

            for (int i = 0; i < hitEvents.Count; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double radius = random.NextDouble() * 0.5f * OsuHitObject.OBJECT_RADIUS;

                var position = new Vector2((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)));

                hitEvents[i] = hitEvents[i].With(position);
            }

            return hitEvents;
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                throw new NotImplementedException();
            }

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            {
                throw new NotImplementedException();
            }

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TestBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
            {
                throw new NotImplementedException();
            }

            public override string Description => string.Empty;

            public override string ShortName => string.Empty;

            protected static Drawable CreatePlaceholderStatistic(string message) => new Container
            {
                RelativeSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = 20,
                Height = 250,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.5f),
                        Alpha = 0.5f
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Text = message,
                        Margin = new MarginPadding { Left = 20 }
                    }
                }
            };

            private class TestBeatmapConverter : IBeatmapConverter
            {
#pragma warning disable CS0067 // The event is never used
                public event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;
#pragma warning restore CS0067

                public IBeatmap Beatmap { get; }

                public TestBeatmapConverter(IBeatmap beatmap)
                {
                    Beatmap = beatmap;
                }

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default) => Beatmap.Clone();
            }
        }

        private class TestRulesetAllStatsRequireHitEvents : TestRuleset
        {
            public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => new[]
            {
                new StatisticItem("Statistic Requiring Hit Events 1", () => CreatePlaceholderStatistic("Placeholder statistic. Requires hit events"), true),
                new StatisticItem("Statistic Requiring Hit Events 2", () => CreatePlaceholderStatistic("Placeholder statistic. Requires hit events"), true)
            };
        }

        private class TestRulesetNoStatsRequireHitEvents : TestRuleset
        {
            public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
            {
                return new[]
                {
                    new StatisticItem("Statistic Not Requiring Hit Events 1", () => CreatePlaceholderStatistic("Placeholder statistic. Does not require hit events")),
                    new StatisticItem("Statistic Not Requiring Hit Events 2", () => CreatePlaceholderStatistic("Placeholder statistic. Does not require hit events"))
                };
            }
        }

        private class TestRulesetMixed : TestRuleset
        {
            public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
            {
                return new[]
                {
                    new StatisticItem("Statistic Requiring Hit Events", () => CreatePlaceholderStatistic("Placeholder statistic. Requires hit events"), true),
                    new StatisticItem("Statistic Not Requiring Hit Events", () => CreatePlaceholderStatistic("Placeholder statistic. Does not require hit events"))
                };
            }
        }
    }
}
