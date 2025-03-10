// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.SelectV2.Leaderboards;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneLeaderboardScore : SongSelectComponentsTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private FillFlowContainer? fillFlow;
        private OsuSpriteText? drawWidthText;

        [Test]
        public void TestSheared()
        {
            AddStep("create content", () =>
            {
                Children = new Drawable[]
                {
                    fillFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 2f),
                        Shear = new Vector2(OsuGame.SHEAR, 0)
                    },
                    drawWidthText = new OsuSpriteText(),
                };

                foreach (var scoreInfo in getTestScores())
                {
                    fillFlow.Add(new LeaderboardScoreV2(scoreInfo)
                    {
                        Rank = scoreInfo.Position,
                        IsPersonalBest = scoreInfo.User.Id == 2,
                        Shear = Vector2.Zero,
                    });
                }

                foreach (var score in fillFlow.Children)
                    score.Show();
            });
        }

        [Test]
        public void TestNonSheared()
        {
            AddStep("create content", () =>
            {
                Children = new Drawable[]
                {
                    fillFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 2f),
                    },
                    drawWidthText = new OsuSpriteText(),
                };

                foreach (var scoreInfo in getTestScores())
                {
                    fillFlow.Add(new LeaderboardScoreV2(scoreInfo)
                    {
                        Rank = scoreInfo.Position,
                        IsPersonalBest = scoreInfo.User.Id == 2,
                    });
                }

                foreach (var score in fillFlow.Children)
                    score.Show();
            });
        }

        [Test]
        public void TestUseTheseModsDoesNotCopySystemMods()
        {
            LeaderboardScoreV2 score = null!;

            AddStep("create content", () =>
            {
                Children = new Drawable[]
                {
                    fillFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 2f),
                        Shear = new Vector2(OsuGame.SHEAR, 0)
                    },
                    drawWidthText = new OsuSpriteText(),
                };

                var scoreInfo = new ScoreInfo
                {
                    Position = 999,
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = RNG.Next(1_800_000, 2_000_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Mods = new Mod[] { new OsuModHidden(), new ModScoreV2(), },
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        CountryCode = CountryCode.ES,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    },
                    Date = DateTimeOffset.Now.AddYears(-2),
                };

                fillFlow.Add(score = new LeaderboardScoreV2(scoreInfo)
                {
                    Rank = scoreInfo.Position,
                    Shear = Vector2.Zero,
                });

                score.Show();
            });
            AddStep("right click panel", () =>
            {
                InputManager.MoveMouseTo(score);
                InputManager.Click(MouseButton.Right);
            });
            AddStep("click use these mods", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableOsuMenuItem>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("mods received HD", () => score.SelectedMods.Value.Any(m => m is OsuModHidden));
            AddAssert("mods did not receive SV2", () => !score.SelectedMods.Value.Any(m => m is ModScoreV2));
        }

        public override void SetUpSteps()
        {
            AddToggleStep("toggle scoring mode", v => config.SetValue(OsuSetting.ScoreDisplayMode, v ? ScoringMode.Classic : ScoringMode.Standardised));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (drawWidthText != null) drawWidthText.Text = $"DrawWidth: {fillFlow?.DrawWidth}";
        }

        private static ScoreInfo[] getTestScores()
        {
            var scores = new[]
            {
                new ScoreInfo
                {
                    Position = 999,
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = RNG.Next(1_800_000, 2_000_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        CountryCode = CountryCode.ES,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    },
                    Date = DateTimeOffset.Now.AddYears(-2),
                },
                new ScoreInfo
                {
                    Position = 22333,
                    Rank = ScoreRank.S,
                    Accuracy = 0.1f,
                    MaxCombo = 32040,
                    TotalScore = RNG.Next(1_200_000, 1_500_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        CountryCode = CountryCode.CA,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    },
                    Date = DateTimeOffset.Now.AddMonths(-6),
                },
                TestResources.CreateTestScoreInfo(),
                new ScoreInfo
                {
                    Position = 110000,
                    Rank = ScoreRank.B,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = RNG.Next(1_000_000, 1_200_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Ruleset = new ManiaRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Username = @"No cover",
                        CountryCode = CountryCode.BR,
                    },
                    Date = DateTimeOffset.Now,
                },
                new ScoreInfo
                {
                    Position = 110000,
                    Rank = ScoreRank.D,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = RNG.Next(500_000, 1_000_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Ruleset = new ManiaRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 226597,
                        Username = @"WWWWWWWWWWWWWWWWWWWW",
                        CountryCode = CountryCode.US,
                    },
                    Date = DateTimeOffset.Now,
                },
            };

            scores[2].Rank = ScoreRank.A;
            scores[2].TotalScore = RNG.Next(120_000, 400_000);
            scores[2].MaximumStatistics[HitResult.Great] = 3000;

            scores[1].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight() };
            scores[2].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight(), new OsuModClassic() };
            scores[3].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight(), new OsuModClassic(), new OsuModDifficultyAdjust() };
            scores[4].Mods = new ManiaRuleset().CreateAllMods().ToArray();

            return scores;
        }
    }
}
