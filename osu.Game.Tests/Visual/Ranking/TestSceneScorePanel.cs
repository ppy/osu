// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Expanded;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanel : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ScorePanel),
            typeof(PanelState),
            typeof(ExpandedPanelMiddleContent),
            typeof(ExpandedPanelTopContent),
        };

        [Test]
        public void TestDRank()
        {
            var score = createScore();
            score.Accuracy = 0.5;
            score.Rank = ScoreRank.D;

            addPanelStep(score);
        }

        [Test]
        public void TestCRank()
        {
            var score = createScore();
            score.Accuracy = 0.75;
            score.Rank = ScoreRank.C;

            addPanelStep(score);
        }

        [Test]
        public void TestBRank()
        {
            var score = createScore();
            score.Accuracy = 0.85;
            score.Rank = ScoreRank.B;

            addPanelStep(score);
        }

        [Test]
        public void TestARank()
        {
            var score = createScore();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addPanelStep(score);
        }

        [Test]
        public void TestSRank()
        {
            var score = createScore();
            score.Accuracy = 0.975;
            score.Rank = ScoreRank.S;

            addPanelStep(score);
        }

        [Test]
        public void TestAlmostSSRank()
        {
            var score = createScore();
            score.Accuracy = 0.9999;
            score.Rank = ScoreRank.S;

            addPanelStep(score);
        }

        [Test]
        public void TestSSRank()
        {
            var score = createScore();
            score.Accuracy = 1;
            score.Rank = ScoreRank.X;

            addPanelStep(score);
        }

        [Test]
        public void TestAllHitResults()
        {
            var score = createScore();
            score.Statistics[HitResult.Perfect] = 350;
            score.Statistics[HitResult.Ok] = 200;

            addPanelStep(score);
        }

        private void addPanelStep(ScoreInfo score) => AddStep("add panel", () =>
        {
            Child = new ScorePanel(score)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = PanelState.Expanded
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
