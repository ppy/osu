// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        [TestCase(0.2, ScoreRank.D)]
        [TestCase(0.5, ScoreRank.D)]
        [TestCase(0.75, ScoreRank.C)]
        [TestCase(0.85, ScoreRank.B)]
        [TestCase(0.925, ScoreRank.A)]
        [TestCase(0.975, ScoreRank.S)]
        [TestCase(0.9999, ScoreRank.S)]
        [TestCase(1, ScoreRank.X)]
        public void TestRank(double accuracy, ScoreRank rank)
        {
            var score = createScore(accuracy, rank);

            addCircleStep(score);
        }

        private void addCircleStep(ScoreInfo score) => AddStep("add panel", () =>
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

        private ScoreInfo createScore(double accuracy, ScoreRank rank) => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = accuracy,
            MaxCombo = 999,
            Rank = rank,
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
