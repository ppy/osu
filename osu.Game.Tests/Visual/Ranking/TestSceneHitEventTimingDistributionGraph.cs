// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneHitEventTimingDistributionGraph : OsuTestScene
    {
        [Test]
        public void TestManyDistributedEvents()
        {
            createTest(CreateDistributedHitEvents());
        }

        [Test]
        public void TestAroundCentre()
        {
            createTest(Enumerable.Range(-150, 300).Select(i => new HitEvent(i / 50f, HitResult.Perfect, new HitCircle(), new HitCircle(), null)).ToList());
        }

        [Test]
        public void TestZeroTimeOffset()
        {
            createTest(Enumerable.Range(0, 100).Select(_ => new HitEvent(0, HitResult.Perfect, new HitCircle(), new HitCircle(), null)).ToList());
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
                    return new HitEvent(0, HitResult.Perfect, new HitCircle(), new HitCircle(), null);

                return new HitEvent(30, HitResult.Miss, new HitCircle(), new HitCircle(), null);
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
                new HitEventTimingDistributionGraph(events)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(600, 130)
                }
            };
        });

        public static List<HitEvent> CreateDistributedHitEvents()
        {
            var hitEvents = new List<HitEvent>();

            for (int i = 0; i < 50; i++)
            {
                int count = (int)(Math.Pow(25 - Math.Abs(i - 25), 2));

                for (int j = 0; j < count; j++)
                    hitEvents.Add(new HitEvent(i - 25, HitResult.Perfect, new HitCircle(), new HitCircle(), null));
            }

            return hitEvents;
        }
    }
}
