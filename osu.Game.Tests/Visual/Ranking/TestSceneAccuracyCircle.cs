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
        [Test]
        public void TestLowDRank()
        {
            var score = createScore();
            score.Accuracy = 0.2;
            score.Rank = ScoreRank.D;

            addCircleStep(score);
        }

        [Test]
        public void TestDRank()
        {
            var score = createScore();
            score.Accuracy = 0.5;
            score.Rank = ScoreRank.D;

            addCircleStep(score);
        }

        [Test]
        public void TestCRank()
        {
            var score = createScore();
            score.Accuracy = 0.75;
            score.Rank = ScoreRank.C;

            addCircleStep(score);
        }

        [Test]
        public void TestBRank()
        {
            var score = createScore();
            score.Accuracy = 0.85;
            score.Rank = ScoreRank.B;

            addCircleStep(score);
        }

        [Test]
        public void TestARank()
        {
            var score = createScore();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addCircleStep(score);
        }

        [Test]
        public void TestSRank()
        {
            var score = createScore();
            score.Accuracy = 0.975;
            score.Rank = ScoreRank.S;

            addCircleStep(score);
        }

        [Test]
        public void TestAlmostSSRank()
        {
            var score = createScore();
            score.Accuracy = 0.9999;
            score.Rank = ScoreRank.S;

            addCircleStep(score);
        }

        [Test]
        public void TestSSRank()
        {
            var score = createScore();
            score.Accuracy = 1;
            score.Rank = ScoreRank.X;

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

        private ScoreInfo createScore() => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = 0.95,
            MaxCombo = 999,
            Rank = ScoreRank.S,
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
