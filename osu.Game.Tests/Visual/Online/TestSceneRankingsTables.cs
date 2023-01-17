// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Rankings.Tables;
using osu.Framework.Graphics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneRankingsTables : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        private readonly BasicScrollContainer scrollFlow;
        private readonly LoadingLayer loading;
        private CancellationTokenSource cancellationToken;

        public TestSceneRankingsTables()
        {
            Children = new Drawable[]
            {
                scrollFlow = new BasicScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                },
                loading = new LoadingLayer(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("User performance", createPerformanceTable);
            AddStep("User scores", createScoreTable);
            AddStep("Country scores", createCountryTable);
        }

        private void createCountryTable()
        {
            onLoadStarted();

            var countries = new List<CountryStatistics>
            {
                new CountryStatistics
                {
                    Code = CountryCode.US,
                    ActiveUsers = 2_972_623,
                    PlayCount = 3_086_515_743,
                    RankedScore = 449_407_643_332_546,
                    Performance = 371_974_024
                },
                new CountryStatistics
                {
                    Code = CountryCode.RU,
                    ActiveUsers = 1_609_989,
                    PlayCount = 1_637_052_841,
                    RankedScore = 221_660_827_473_004,
                    Performance = 163_426_476
                }
            };

            var table = new CountriesTable(1, countries);
            loadTable(table);
        }

        private static List<UserStatistics> createUserStatistics() => new List<UserStatistics>
        {
            new UserStatistics
            {
                User = new APIUser
                {
                    Username = "first active user",
                    CountryCode = CountryCode.JP,
                    Active = true,
                },
                Accuracy = 0.9972,
                PlayCount = 233_215,
                TotalScore = 983_231_234_656,
                RankedScore = 593_231_345_897,
                PP = 23_934,
                GradesCount = new UserStatistics.Grades
                {
                    SS = 35_132,
                    S = 23_345,
                    A = 12_234
                }
            },
            new UserStatistics
            {
                User = new APIUser
                {
                    Username = "inactive user",
                    CountryCode = CountryCode.AU,
                    Active = false,
                },
                Accuracy = 0.9831,
                PlayCount = 195_342,
                TotalScore = 683_231_234_656,
                RankedScore = 393_231_345_897,
                PP = 20_934,
                GradesCount = new UserStatistics.Grades
                {
                    SS = 32_132,
                    S = 20_345,
                    A = 9_234
                }
            },
            new UserStatistics
            {
                User = new APIUser
                {
                    Username = "second active user",
                    CountryCode = CountryCode.PL,
                    Active = true,
                },
                Accuracy = 0.9584,
                PlayCount = 100_903,
                TotalScore = 97_242_983_434,
                RankedScore = 3_156_345_897,
                PP = 9_568,
                GradesCount = new UserStatistics.Grades
                {
                    SS = 13_152,
                    S = 24_375,
                    A = 9_960
                }
            },
        };

        private void createPerformanceTable()
        {
            onLoadStarted();
            loadTable(new PerformanceTable(1, createUserStatistics()));
        }

        private void createScoreTable()
        {
            onLoadStarted();
            loadTable(new ScoresTable(1, createUserStatistics()));
        }

        private void onLoadStarted()
        {
            loading.Show();
            cancellationToken?.Cancel();
            cancellationToken = new CancellationTokenSource();
        }

        private void loadTable(Drawable table)
        {
            LoadComponentAsync(table, t =>
            {
                scrollFlow.Clear();
                scrollFlow.Add(t);
                loading.Hide();
            }, cancellationToken.Token);
        }
    }
}
