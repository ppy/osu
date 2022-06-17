// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneHitEventTimingDistributionGraph : OsuTestScene
    {
        private HitEventTimingDistributionGraph graph;

        private static readonly HitObject placeholder_object = new HitCircle();

        [Test]
        public void TestManyDistributedEvents()
        {
            createTest(CreateDistributedHitEvents());
            AddStep("add adjustment", () => graph.UpdateOffset(10));
        }

        [Test]
        public void TestManyDistributedEventsOffset()
        {
            createTest(CreateDistributedHitEvents(-3.5));
        }

        [Test]
        public void TestAroundCentre()
        {
            createTest(Enumerable.Range(-150, 300).Select(i => new HitEvent(i / 50f, HitResult.Perfect, placeholder_object, placeholder_object, null)).ToList());
        }

        [Test]
        public void TestZeroTimeOffset()
        {
            createTest(Enumerable.Range(0, 100).Select(_ => new HitEvent(0, HitResult.Perfect, placeholder_object, placeholder_object, null)).ToList());
        }

        [Test]
        public void TestNoEvents()
        {
            createTest(new List<HitEvent>());
        }

        [Test]
        public void TestMissesDontShow()
        {
            createTest(Enumerable.Range(0, 100).Select(i =>
            {
                if (i % 2 == 0)
                    return new HitEvent(0, HitResult.Perfect, placeholder_object, placeholder_object, null);

                return new HitEvent(30, HitResult.Miss, placeholder_object, placeholder_object, null);
            }).ToList());
        }

        private void createTest(List<HitEvent> events) => AddStep("create test", () =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333")
                },
                graph = new HitEventTimingDistributionGraph(events)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(600, 130)
                }
            };
        });

        public static List<HitEvent> CreateDistributedHitEvents(double centre = 0, double range = 25)
        {
            var hitEvents = new List<HitEvent>();

            for (int i = 0; i < range * 2; i++)
            {
                int count = (int)(Math.Pow(range - Math.Abs(i - range), 2)) / 10;

                for (int j = 0; j < count; j++)
                    hitEvents.Add(new HitEvent(centre + i - range, HitResult.Perfect, placeholder_object, placeholder_object, null));
            }

            return hitEvents;
        }
    }
}
