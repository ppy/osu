// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
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
    public class TestSceneScoresContainer : OsuTestScene
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

        private int onlineID = 1;

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
                    },
                }
            };

            const int initial_great_count = 2000;

            int greatCount = initial_great_count;

            foreach (var s in scores.Scores)
            {
                s.Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Great, greatCount -= 100 },
                    { HitResult.Ok, RNG.Next(100) },
                    { HitResult.Meh, RNG.Next(100) },
                    { HitResult.Miss, initial_great_count - greatCount }
                };
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
            },
            Position = 1337,
        };

        private class TestScoresContainer : ScoresContainer
        {
            public new APIScoresCollection Scores
            {
                set => base.Scores = value;
            }
        }
    }
}
