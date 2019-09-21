// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
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
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
                            new OsuModHidden(),
                            new OsuModFlashlight(),
                            new OsuModHardRock(),
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
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
                            new OsuModHidden(),
                            new OsuModFlashlight(),
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
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
                            new OsuModHidden(),
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
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
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
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
                            new OsuModHidden(),
                            new OsuModFlashlight(),
                            new OsuModHardRock(),
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
                s.Statistics.Add(HitResult.Great, RNG.Next(2000));
                s.Statistics.Add(HitResult.Good, RNG.Next(2000));
                s.Statistics.Add(HitResult.Meh, RNG.Next(2000));
                s.Statistics.Add(HitResult.Miss, RNG.Next(2000));
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
