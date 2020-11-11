// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneStatisticsPanel : OsuTestScene
    {
        [Test]
        public void TestScoreWithTimeStatistics()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo)
            {
                HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents()
            };

            loadPanel(score);
        }

        [Test]
        public void TestScoreWithPositionStatistics()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo)
            {
                HitEvents = CreatePositionDistributedHitEvents()
            };

            loadPanel(score);
        }

        [Test]
        public void TestScoreWithoutStatistics()
        {
            loadPanel(new TestScoreInfo(new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestNullScore()
        {
            loadPanel(null);
        }

        private void loadPanel(ScoreInfo score) => AddStep("load panel", () =>
        {
            Child = new StatisticsPanel
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                Score = { Value = score }
            };
        });

        public static List<HitEvent> CreatePositionDistributedHitEvents()
        {
            var hitEvents = new List<HitEvent>();
            // Use constant seed for reproducibility
            var random = new Random(0);

            for (int i = 0; i < 500; i++)
            {
                float angle = (float)random.NextDouble() * 2 * (float)Math.PI;
                float radius = (float)random.NextDouble() * 0.5f * OsuHitObject.OBJECT_RADIUS;

                Vector2 position = new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));

                hitEvents.Add(new HitEvent(0, HitResult.Perfect, new HitCircle(), new HitCircle(), position));
            }

            return hitEvents;
        }
    }
}
