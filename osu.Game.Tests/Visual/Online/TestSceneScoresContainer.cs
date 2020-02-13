// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneScoresContainer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableTopScore),
            typeof(TopScoreUserSection),
            typeof(TopScoreStatisticsSection),
            typeof(ScoreTable),
            typeof(ScoreTableRowBackground),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneScoresContainer()
        {
            TestScoresContainer scoresContainer;

            Child = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Width = 0.8f,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    scoresContainer = new TestScoresContainer(),
                }
            };

            var allScores = new APILegacyScores
            {
                Scores = new List<APILegacyScoreInfo>
                {
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 6602580,
                            Username = @"waaiiru",
                            Country = new Country
                            {
                                FullName = @"Spain",
                                FlagName = @"ES",
                            },
                        },
                        Mods = new[]
                        {
                            new OsuModDoubleTime().Acronym,
                            new OsuModHidden().Acronym,
                            new OsuModFlashlight().Acronym,
                            new OsuModHardRock().Acronym,
                        },
                        Rank = ScoreRank.XH,
                        PP = 200,
                        MaxCombo = 1234,
                        TotalScore = 1234567890,
                        Accuracy = 1,
                    },
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 4608074,
                            Username = @"Skycries",
                            Country = new Country
                            {
                                FullName = @"Brazil",
                                FlagName = @"BR",
                            },
                        },
                        Mods = new[]
                        {
                            new OsuModDoubleTime().Acronym,
                            new OsuModHidden().Acronym,
                            new OsuModFlashlight().Acronym,
                        },
                        Rank = ScoreRank.S,
                        PP = 190,
                        MaxCombo = 1234,
                        TotalScore = 1234789,
                        Accuracy = 0.9997,
                    },
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 1014222,
                            Username = @"eLy",
                            Country = new Country
                            {
                                FullName = @"Japan",
                                FlagName = @"JP",
                            },
                        },
                        Mods = new[]
                        {
                            new OsuModDoubleTime().Acronym,
                            new OsuModHidden().Acronym,
                        },
                        Rank = ScoreRank.B,
                        PP = 180,
                        MaxCombo = 1234,
                        TotalScore = 12345678,
                        Accuracy = 0.9854,
                    },
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 1541390,
                            Username = @"Toukai",
                            Country = new Country
                            {
                                FullName = @"Canada",
                                FlagName = @"CA",
                            },
                        },
                        Mods = new[]
                        {
                            new OsuModDoubleTime().Acronym,
                        },
                        Rank = ScoreRank.C,
                        PP = 170,
                        MaxCombo = 1234,
                        TotalScore = 1234567,
                        Accuracy = 0.8765,
                    },
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 7151382,
                            Username = @"Mayuri Hana",
                            Country = new Country
                            {
                                FullName = @"Thailand",
                                FlagName = @"TH",
                            },
                        },
                        Rank = ScoreRank.D,
                        PP = 160,
                        MaxCombo = 1234,
                        TotalScore = 123456,
                        Accuracy = 0.6543,
                    },
                }
            };

            var myBestScore = new APILegacyUserTopScoreInfo
            {
                Score = new APILegacyScoreInfo
                {
                    User = new User
                    {
                        Id = 7151382,
                        Username = @"Mayuri Hana",
                        Country = new Country
                        {
                            FullName = @"Thailand",
                            FlagName = @"TH",
                        },
                    },
                    Rank = ScoreRank.D,
                    PP = 160,
                    MaxCombo = 1234,
                    TotalScore = 123456,
                    Accuracy = 0.6543,
                },
                Position = 1337,
            };

            var oneScore = new APILegacyScores
            {
                Scores = new List<APILegacyScoreInfo>
                {
                    new APILegacyScoreInfo
                    {
                        User = new User
                        {
                            Id = 6602580,
                            Username = @"waaiiru",
                            Country = new Country
                            {
                                FullName = @"Spain",
                                FlagName = @"ES",
                            },
                        },
                        Mods = new[]
                        {
                            new OsuModDoubleTime().Acronym,
                            new OsuModHidden().Acronym,
                            new OsuModFlashlight().Acronym,
                            new OsuModHardRock().Acronym,
                        },
                        Rank = ScoreRank.XH,
                        PP = 200,
                        MaxCombo = 1234,
                        TotalScore = 1234567890,
                        Accuracy = 1,
                    }
                }
            };

            foreach (var s in allScores.Scores)
            {
                s.Statistics = new Dictionary<string, int>
                {
                    { "count_300", RNG.Next(2000) },
                    { "count_100", RNG.Next(2000) },
                    { "count_50", RNG.Next(2000) },
                    { "count_miss", RNG.Next(2000) }
                };
            }

            AddStep("Load all scores", () =>
            {
                allScores.UserScore = null;
                scoresContainer.Scores = allScores;
            });
            AddStep("Load null scores", () => scoresContainer.Scores = null);
            AddStep("Load only one score", () => scoresContainer.Scores = oneScore);
            AddStep("Load scores with my best", () =>
            {
                allScores.UserScore = myBestScore;
                scoresContainer.Scores = allScores;
            });
        }

        private class TestScoresContainer : ScoresContainer
        {
            public new APILegacyScores Scores
            {
                set => base.Scores = value;
            }
        }
    }
}
