// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
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
    public partial class TestSceneHitEventTimingDistributionGraph : OsuTestScene
    {
        private HitEventTimingDistributionGraph graph = null!;
        private readonly BindableFloat width = new BindableFloat(600);
        private readonly BindableFloat height = new BindableFloat(130);

        private static readonly HitObject placeholder_object = new HitCircle();

        public TestSceneHitEventTimingDistributionGraph()
        {
            width.BindValueChanged(e => graph.Width = e.NewValue);
            height.BindValueChanged(e => graph.Height = e.NewValue);
        }

        [Test]
        public void TestManyDistributedEvents()
        {
            createTest(CreateDistributedHitEvents());
            AddStep("add adjustment", () => graph.UpdateOffset(10));
            AddSliderStep("width", 0.0f, 1000.0f, width.Value, width.Set);
            AddSliderStep("height", 0.0f, 1000.0f, height.Value, height.Set);
        }

        [Test]
        public void TestManyDistributedEventsOffset()
        {
            createTest(CreateDistributedHitEvents(-3.5));
        }

        [Test]
        public void TestAroundCentre()
        {
            createTest(Enumerable.Range(-150, 300).Select(i => new HitEvent(i / 50f, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null)).ToList());
        }

        [Test]
        public void TestSparse()
        {
            createTest(new List<HitEvent>
            {
                new HitEvent(-7, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
                new HitEvent(-6, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
                new HitEvent(-5, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
                new HitEvent(5, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
                new HitEvent(6, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
                new HitEvent(7, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null),
            });
        }

        [Test]
        public void TestVariousTypesOfHitResult()
        {
            createTest(CreateDistributedHitEvents(0, 50).Select(h =>
            {
                double offset = Math.Abs(h.TimeOffset);
                HitResult result = offset > 36 ? HitResult.Miss
                    : offset > 32 ? HitResult.Meh
                    : offset > 24 ? HitResult.Ok
                    : offset > 16 ? HitResult.Good
                    : offset > 8 ? HitResult.Great
                    : HitResult.Perfect;
                return new HitEvent(h.TimeOffset, 1.0, result, placeholder_object, placeholder_object, null);
            }).ToList());
        }

        [Test]
        public void TestNonBasicHitResultsAreIgnored()
        {
            createTest(CreateDistributedHitEvents(0, 50)
                       .Select(h => new HitEvent(h.TimeOffset, 1.0, h.TimeOffset > 0 ? HitResult.Ok : HitResult.LargeTickHit, placeholder_object, placeholder_object, null))
                       .ToList());
        }

        [Test]
        public void TestMultipleWindowsOfHitResult()
        {
            var wide = CreateDistributedHitEvents(0, 50).Select(h =>
            {
                double offset = Math.Abs(h.TimeOffset);
                HitResult result = offset > 36 ? HitResult.Miss
                    : offset > 32 ? HitResult.Meh
                    : offset > 24 ? HitResult.Ok
                    : offset > 16 ? HitResult.Good
                    : offset > 8 ? HitResult.Great
                    : HitResult.Perfect;

                return new HitEvent(h.TimeOffset, 1.0, result, placeholder_object, placeholder_object, null);
            });
            var narrow = CreateDistributedHitEvents(0, 50).Select(h =>
            {
                double offset = Math.Abs(h.TimeOffset);
                HitResult result = offset > 25 ? HitResult.Miss
                    : offset > 20 ? HitResult.Meh
                    : offset > 15 ? HitResult.Ok
                    : offset > 10 ? HitResult.Good
                    : offset > 5 ? HitResult.Great
                    : HitResult.Perfect;
                return new HitEvent(h.TimeOffset, 1.0, result, placeholder_object, placeholder_object, null);
            });
            createTest(wide.Concat(narrow).ToList());
        }

        [Test]
        public void TestZeroTimeOffset()
        {
            createTest(Enumerable.Range(0, 100).Select(_ => new HitEvent(0, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null)).ToList());
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
                    return new HitEvent(0, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null);

                return new HitEvent(30, 1.0, HitResult.Miss, placeholder_object, placeholder_object, null);
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
                    Size = new Vector2(width.Value, height.Value)
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
                    hitEvents.Add(new HitEvent(centre + i - range, 1.0, HitResult.Perfect, placeholder_object, placeholder_object, null));
            }

            return hitEvents;
        }
    }
}
