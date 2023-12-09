// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneAccuracyCircle : OsuTestScene
    {
        [Test]
        public void TestOsuRank()
        {
            addCircleStep(createScore(0, new OsuRuleset()));
            addCircleStep(createScore(0.5, new OsuRuleset()));
            addCircleStep(createScore(0.699, new OsuRuleset()));
            addCircleStep(createScore(0.7, new OsuRuleset()));
            addCircleStep(createScore(0.75, new OsuRuleset()));
            addCircleStep(createScore(0.799, new OsuRuleset()));
            addCircleStep(createScore(0.8, new OsuRuleset()));
            addCircleStep(createScore(0.85, new OsuRuleset()));
            addCircleStep(createScore(0.899, new OsuRuleset()));
            addCircleStep(createScore(0.9, new OsuRuleset()));
            addCircleStep(createScore(0.925, new OsuRuleset()));
            addCircleStep(createScore(0.9499, new OsuRuleset()));
            addCircleStep(createScore(0.95, new OsuRuleset()));
            addCircleStep(createScore(0.975, new OsuRuleset()));
            addCircleStep(createScore(0.99, new OsuRuleset()));
            addCircleStep(createScore(1, new OsuRuleset()));
        }

        [Test]
        public void TestCatchRank()
        {
            addCircleStep(createScore(0, new CatchRuleset()));
            addCircleStep(createScore(0.5, new CatchRuleset()));
            addCircleStep(createScore(0.8499, new CatchRuleset()));
            addCircleStep(createScore(0.85, new CatchRuleset()));
            addCircleStep(createScore(0.875, new CatchRuleset()));
            addCircleStep(createScore(0.899, new CatchRuleset()));
            addCircleStep(createScore(0.9, new CatchRuleset()));
            addCircleStep(createScore(0.925, new CatchRuleset()));
            addCircleStep(createScore(0.9399, new CatchRuleset()));
            addCircleStep(createScore(0.94, new CatchRuleset()));
            addCircleStep(createScore(0.9675, new CatchRuleset()));
            addCircleStep(createScore(0.9799, new CatchRuleset()));
            addCircleStep(createScore(0.98, new CatchRuleset()));
            addCircleStep(createScore(0.99, new CatchRuleset()));
            addCircleStep(createScore(1, new CatchRuleset()));
        }

        private void addCircleStep(ScoreInfo score) => AddStep($"add panel ({score.DisplayAccuracy})", () =>
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 700),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"))
                        }
                    }
                },
                new AccuracyCircle(score, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(230)
                }
            };
        });

        private ScoreInfo createScore(double accuracy, Ruleset ruleset)
        {
            var scoreProcessor = ruleset.CreateScoreProcessor();

            return new ScoreInfo
            {
                User = new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                BeatmapInfo = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
                Ruleset = ruleset.RulesetInfo,
                Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
                TotalScore = 2845370,
                Accuracy = accuracy,
                MaxCombo = 999,
                Rank = scoreProcessor.RankFromAccuracy(accuracy),
                Date = DateTimeOffset.Now,
                Statistics =
                {
                    { HitResult.Miss, 1 },
                    { HitResult.Meh, 50 },
                    { HitResult.Good, 100 },
                    { HitResult.Great, 300 },
                }
            };
        }
    }
}
