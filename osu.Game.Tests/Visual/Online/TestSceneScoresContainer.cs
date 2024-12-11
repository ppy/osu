// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK.Graphics;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneScoresContainer : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TestScoresContainer scoresContainer;

        [SetUpSteps]
        public void SetUp() => Schedule(() =>
        {
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
                    scoresContainer = new TestScoresContainer
                    {
                        Beatmap = { Value = CreateAPIBeatmap() }
                    }
                }
            };
        });

        [Test]
        public void TestNoUserBest()
        {
            AddStep("Scores with no user best", () =>
            {
                var allScores = createScores();

                allScores.UserScore = null;

                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("no user best displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 1);

            AddStep("Load null scores", () => scoresContainer.Scores = null);

            AddUntilStep("wait for scores not displayed", () => !scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("no best score displayed", () => !scoresContainer.ChildrenOfType<DrawableTopScore>().Any());

            AddStep("Load only one score", () =>
            {
                var allScores = createScores();

                allScores.Scores.RemoveRange(1, allScores.Scores.Count - 1);

                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores not displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Count() == 1);
            AddAssert("no best score displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 1);
        }

        [Test]
        public void TestHitResultsWithSameNameAreGrouped()
        {
            AddStep("Load scores without user best", () =>
            {
                var allScores = createScores();
                allScores.UserScore = null;
                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("only one column for slider end", () =>
            {
                ScoreTable scoreTable = scoresContainer.ChildrenOfType<ScoreTable>().First();
                return scoreTable.Columns.Count(c => c.Header.Equals("slider end")) == 1;
            });

            AddAssert("all rows show non-zero slider ends", () =>
            {
                ScoreTable scoreTable = scoresContainer.ChildrenOfType<ScoreTable>().First();
                int sliderEndColumnIndex = Array.FindIndex(scoreTable.Columns, c => c != null && c.Header.Equals("slider end"));
                bool sliderEndFilledInEachRow = true;

                for (int i = 0; i < scoreTable.Content?.GetLength(0); i++)
                {
                    switch (scoreTable.Content[i, sliderEndColumnIndex])
                    {
                        case OsuSpriteText text:
                            if (text.Text.Equals(0.0d.ToLocalisableString(@"N0")))
                                sliderEndFilledInEachRow = false;
                            break;

                        default:
                            sliderEndFilledInEachRow = false;
                            break;
                    }
                }

                return sliderEndFilledInEachRow;
            });
        }

        [Test]
        public void TestUserBest()
        {
            AddStep("Load scores with personal best", () =>
            {
                var allScores = createScores();
                allScores.UserScore = createUserBest();
                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("best score displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 2);

            AddStep("Load scores with personal best FC", () =>
            {
                var allScores = createScores();
                allScores.UserScore = createUserBest();
                allScores.UserScore.Score.Accuracy = 1;
                scoresContainer.Beatmap.Value.MaxCombo = allScores.UserScore.Score.MaxCombo = 1337;
                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("best score displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 2);

            AddStep("Load scores with personal best (null position)", () =>
            {
                var allScores = createScores();
                var userBest = createUserBest();
                userBest.Position = null;
                allScores.UserScore = userBest;
                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("best score displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 2);

            AddStep("Load scores with personal best (first place)", () =>
            {
                var allScores = createScores();
                allScores.UserScore = new APIScoreWithPosition
                {
                    Score = allScores.Scores.First(),
                    Position = 1,
                };
                scoresContainer.Scores = allScores;
            });

            AddUntilStep("wait for scores displayed", () => scoresContainer.ChildrenOfType<ScoreTableRowBackground>().Any());
            AddAssert("best score displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 1);

            AddStep("Scores with no user best", () =>
            {
                var allScores = createScores();

                allScores.UserScore = null;

                scoresContainer.Scores = allScores;
            });

            AddUntilStep("best score not displayed", () => scoresContainer.ChildrenOfType<DrawableTopScore>().Count() == 1);
        }

        [Test]
        public void TestUnprocessedPP()
        {
            AddStep("Load scores with unprocessed PP", () =>
            {
                var allScores = createScores();
                allScores.Scores[0].PP = null;
                allScores.UserScore = createUserBest();
                allScores.UserScore.Score.PP = null;
                scoresContainer.Scores = allScores;
            });
        }

        [Test]
        public void TestUnrankedPP()
        {
            AddStep("Load scores with unranked PP", () =>
            {
                var allScores = createScores();
                allScores.Scores[0].Ranked = false;
                allScores.UserScore = createUserBest();
                allScores.UserScore.Score.Ranked = false;
                scoresContainer.Scores = allScores;
            });
        }

        private ulong onlineID = 1;

        private APIScoresCollection createScores()
        {
            var scores = new APIScoresCollection
            {
                Scores = new List<SoloScoreInfo>
                {
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 6602580,
                            Username = @"waaiiru",
                            CountryCode = CountryCode.ES,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                            new APIMod { Acronym = new OsuModFlashlight().Acronym },
                            new APIMod { Acronym = new OsuModHardRock().Acronym },
                        },
                        Rank = ScoreRank.XH,
                        PP = 200,
                        MaxCombo = 1234,
                        TotalScore = 1234567890,
                        Accuracy = 1,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 4608074,
                            Username = @"Skycries",
                            CountryCode = CountryCode.BR,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                            new APIMod { Acronym = new OsuModFlashlight().Acronym },
                        },
                        Rank = ScoreRank.S,
                        PP = 190,
                        MaxCombo = 1234,
                        TotalScore = 1234789,
                        Accuracy = 0.9997,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 1014222,
                            Username = @"eLy",
                            CountryCode = CountryCode.JP,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                        },
                        Rank = ScoreRank.B,
                        PP = 180,
                        MaxCombo = 1234,
                        TotalScore = 12345678,
                        Accuracy = 0.9854,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 1541390,
                            Username = @"Toukai",
                            CountryCode = CountryCode.CA,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                        },
                        Rank = ScoreRank.C,
                        PP = 170,
                        MaxCombo = 1234,
                        TotalScore = 1234567,
                        Accuracy = 0.8765,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 7151382,
                            Username = @"Mayuri Hana",
                            CountryCode = CountryCode.TH,
                        },
                        Rank = ScoreRank.D,
                        PP = 160,
                        MaxCombo = 1234,
                        TotalScore = 123456,
                        Accuracy = 0.6543,
                        Ranked = true,
                    },
                }
            };

            const int initial_great_count = 2000;
            const int initial_tick_count = 100;
            const int initial_slider_end_count = 500;

            int greatCount = initial_great_count;
            int tickCount = initial_tick_count;
            int sliderEndCount = initial_slider_end_count;

            foreach (var (score, index) in scores.Scores.Select((s, i) => (s, i)))
            {
                HitResult sliderEndResult = index % 2 == 0 ? HitResult.SliderTailHit : HitResult.SmallTickHit;

                score.Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Great, greatCount },
                    { HitResult.LargeTickHit, tickCount },
                    { HitResult.Ok, RNG.Next(100) },
                    { HitResult.Meh, RNG.Next(100) },
                    { HitResult.Miss, initial_great_count - greatCount },
                    { HitResult.LargeTickMiss, initial_tick_count - tickCount },
                    { sliderEndResult, sliderEndCount },
                };

                // Some hit results, including SliderTailHit and SmallTickHit, are only displayed
                // when the maximum number is known
                score.MaximumStatistics = new Dictionary<HitResult, int>
                {
                    { sliderEndResult, initial_slider_end_count },
                };

                greatCount -= 100;
                tickCount -= RNG.Next(1, 5);
                sliderEndCount -= 20;
            }

            return scores;
        }

        private APIScoreWithPosition createUserBest() => new APIScoreWithPosition
        {
            Score = new SoloScoreInfo
            {
                EndedAt = DateTimeOffset.Now,
                ID = onlineID++,
                User = new APIUser
                {
                    Id = 7151382,
                    Username = @"Mayuri Hana",
                    CountryCode = CountryCode.TH,
                },
                Rank = ScoreRank.D,
                PP = 160,
                MaxCombo = 1234,
                TotalScore = 123456,
                Accuracy = 0.6543,
                Ranked = true,
            },
            Position = 1337,
        };

        private partial class TestScoresContainer : ScoresContainer
        {
            public new APIScoresCollection Scores
            {
                set => base.Scores = value;
            }
        }
    }
}
