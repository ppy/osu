// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.RankingV2.Extended;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using Realms;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneExtendedGlobalStatisticsUpdatePanel : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Cached(typeof(IBindable<ScoreBasedUserStatisticsUpdate>))]
        private Bindable<ScoreBasedUserStatisticsUpdate?> statisticsUpdate = new Bindable<ScoreBasedUserStatisticsUpdate?>();

        [Cached]
        private OverlayColourProvider colours { get; set; } = new OverlayColourProvider(OverlayColourScheme.Blue);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.Run(r =>
            {
                var beatmapInfo = r.All<BeatmapInfo>()
                                   .Filter($"{nameof(BeatmapInfo.Ruleset)}.{nameof(RulesetInfo.OnlineID)} = $0 AND {nameof(BeatmapInfo.StatusInt)} = 1", 0)
                                   .FirstOrDefault();

                if (beatmapInfo != null)
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.4f,
                Padding = new MarginPadding
                {
                    Horizontal = 30,
                    Vertical = 10,
                },
                Child = new GlobalStatisticsUpdatePanel
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });

            AddStep("set loading", () => statisticsUpdate.Value = null);

            AddStep("set update", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = 38000,
                        PP = 2134,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        TotalScore = 128749597,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        GlobalRank = 36000,
                        PP = (decimal)2134.5,
                        RankedScore = 23897015,
                        Accuracy = 0.984,
                        TotalScore = 132218497,
                        MaxCombo = 1233,
                    });
            });

            AddStep("all gains", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = 38000,
                        PP = 2134,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        TotalScore = 128749597,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        GlobalRank = 33000,
                        PP = 2250,
                        RankedScore = 21145834,
                        Accuracy = 0.988,
                        TotalScore = 128944531,
                        MaxCombo = 1244,
                    });
            });

            AddStep("no changes", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                var stats = new UserStatistics
                {
                    GlobalRank = 38000,
                    PP = 2134,
                    RankedScore = 21123849,
                    Accuracy = 0.985,
                    TotalScore = 128749597,
                    MaxCombo = 1233,
                };
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s, stats, stats);
            });

            AddStep("all losses", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = 38000,
                        PP = 2134,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        TotalScore = 128749597,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        GlobalRank = 44000,
                        PP = 2023,
                        RankedScore = 20044352,
                        Accuracy = 0.943,
                        TotalScore = 123894989,
                        MaxCombo = 130,
                    });
            });

            AddStep("had no ranking", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = null,
                        PP = null,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        TotalScore = 128749597,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        GlobalRank = 36000,
                        PP = (decimal)2134.5,
                        RankedScore = 23897015,
                        Accuracy = 0.984,
                        TotalScore = 132218497,
                        MaxCombo = 1233,
                    });
            });

            AddStep("has no ranking", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = null,
                        PP = null,
                        RankedScore = 21123849,
                        Accuracy = 0.985,
                        TotalScore = 128749597,
                        MaxCombo = 1233,
                    }, new UserStatistics
                    {
                        GlobalRank = null,
                        PP = null,
                        RankedScore = 23897015,
                        Accuracy = 0.984,
                        TotalScore = 132218497,
                        MaxCombo = 1233,
                    });
            });

            // cross-reference: `TestSceneToolbarUserButton.TestTransientUserStatisticsDisplay()`, "Test rounding treatment" step.
            AddStep("rounding treatment", () =>
            {
                var s = TestResources.CreateTestScoreInfo(Beatmap.Value.BeatmapInfo);
                statisticsUpdate.Value = new ScoreBasedUserStatisticsUpdate(s,
                    new UserStatistics
                    {
                        GlobalRank = 12_345,
                        Accuracy = 98.99,
                        MaxCombo = 2_322,
                        RankedScore = 23_123_543_456,
                        TotalScore = 123_123_543_456,
                        PP = 5_071.495M
                    },
                    new UserStatistics
                    {
                        GlobalRank = 12_345,
                        Accuracy = 98.99,
                        MaxCombo = 2_322,
                        RankedScore = 23_123_543_456,
                        TotalScore = 123_123_543_456,
                        PP = 5_072.99M
                    });
            });
        }
    }
}
