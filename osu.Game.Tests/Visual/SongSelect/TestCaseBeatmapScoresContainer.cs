// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    [System.ComponentModel.Description("in BeatmapOverlay")]
    public class TestCaseBeatmapScoresContainer : OsuTestCase
    {
        private readonly Box background;

        public TestCaseBeatmapScoresContainer()
        {
            Container container;
            ScoresContainer scoresContainer;

            Child = container = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Width = 0.8f,
                Children = new Drawable[]
                {
                    background = new Box { RelativeSizeAxes = Axes.Both },
                    scoresContainer = new ScoresContainer(),
                }
            };

            IEnumerable<ScoreInfo> scores = new[]
            {
                new ScoreInfo
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
                    TotalScore = 1234567890,
                    Accuracy = 1,
                },
                new ScoreInfo
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
                    TotalScore = 1234789,
                    Accuracy = 0.9997,
                },
                new ScoreInfo
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
                    TotalScore = 12345678,
                    Accuracy = 0.9854,
                },
                new ScoreInfo
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
                    TotalScore = 1234567,
                    Accuracy = 0.8765,
                },
                new ScoreInfo
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
                    Rank = ScoreRank.F,
                    TotalScore = 123456,
                    Accuracy = 0.6543,
                },
            };

            foreach (var s in scores)
            {
                s.Statistics.Add(HitResult.Great, RNG.Next(2000));
                s.Statistics.Add(HitResult.Good, RNG.Next(2000));
                s.Statistics.Add(HitResult.Meh, RNG.Next(2000));
            }

            IEnumerable<ScoreInfo> anotherScores = new[]
            {
                new ScoreInfo
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
                    TotalScore = 1234789,
                    Accuracy = 0.9997,
                },
                new ScoreInfo
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
                    TotalScore = 1234567890,
                    Accuracy = 1,
                },
                new ScoreInfo
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
                    Rank = ScoreRank.F,
                    TotalScore = 123456,
                    Accuracy = 0.6543,
                },
                new ScoreInfo
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
                    TotalScore = 12345678,
                    Accuracy = 0.9854,
                },
                new ScoreInfo
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
                    TotalScore = 1234567,
                    Accuracy = 0.8765,
                },
            };
            foreach (var s in anotherScores)
            {
                s.Statistics.Add(HitResult.Great, RNG.Next(2000));
                s.Statistics.Add(HitResult.Good, RNG.Next(2000));
                s.Statistics.Add(HitResult.Meh, RNG.Next(2000));
            }

            var topScoreInfo = new ScoreInfo
            {
                User = new User
                {
                    Id = 2705430,
                    Username = @"Mooha",
                    Country = new Country
                    {
                        FullName = @"France",
                        FlagName = @"FR",
                    },
                },
                Mods = new Mod[]
                {
                    new OsuModDoubleTime(),
                    new OsuModFlashlight(),
                    new OsuModHardRock(),
                },
                Rank = ScoreRank.B,
                TotalScore = 987654321,
                Accuracy = 0.8487,
            };
            topScoreInfo.Statistics.Add(HitResult.Great, RNG.Next(2000));
            topScoreInfo.Statistics.Add(HitResult.Good, RNG.Next(2000));
            topScoreInfo.Statistics.Add(HitResult.Meh, RNG.Next(2000));

            AddStep("scores pack 1", () => scoresContainer.Scores = scores);
            AddStep("scores pack 2", () => scoresContainer.Scores = anotherScores);
            AddStep("only top score", () => scoresContainer.Scores = new[] { topScoreInfo });
            AddStep("remove scores", () => scoresContainer.Scores = null);
            AddStep("resize to big", () => container.ResizeWidthTo(1, 300));
            AddStep("resize to normal", () => container.ResizeWidthTo(0.8f, 300));
            AddStep("online scores", () => scoresContainer.Beatmap = new BeatmapInfo { OnlineBeatmapID = 75, Ruleset = new OsuRuleset().RulesetInfo });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
        }
    }
}
