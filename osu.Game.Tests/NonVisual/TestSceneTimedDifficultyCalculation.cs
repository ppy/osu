// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class TestSceneTimedDifficultyCalculation
    {
        [Test]
        public void TestAttributesGeneratedForAllNonSkippedObjects()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    new TestHitObject { StartTime = 1 },
                    new TestHitObject
                    {
                        StartTime = 2,
                        Nested = 1
                    },
                    new TestHitObject { StartTime = 3 },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).CalculateTimed();

            Assert.That(attribs.Count, Is.EqualTo(4));
            assertEquals(attribs[0], beatmap.HitObjects[0]);
            assertEquals(attribs[1], beatmap.HitObjects[0], beatmap.HitObjects[1]);
            assertEquals(attribs[2], beatmap.HitObjects[0], beatmap.HitObjects[1]); // From the nested object.
            assertEquals(attribs[3], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        [Test]
        public void TestAttributesNotGeneratedForSkippedObjects()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    // The first object is usually skipped in all implementations
                    new TestHitObject
                    {
                        StartTime = 1,
                        Skip = true
                    },
                    // An intermediate skipped object.
                    new TestHitObject
                    {
                        StartTime = 2,
                        Skip = true
                    },
                    new TestHitObject { StartTime = 3 },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).CalculateTimed();

            Assert.That(attribs.Count, Is.EqualTo(1));
            assertEquals(attribs[0], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        [Test]
        public void TestNestedObjectOnlyAddsParentOnce()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    new TestHitObject
                    {
                        StartTime = 1,
                        Skip = true,
                        Nested = 2
                    },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).CalculateTimed();

            Assert.That(attribs.Count, Is.EqualTo(2));
            assertEquals(attribs[0], beatmap.HitObjects[0]);
            assertEquals(attribs[1], beatmap.HitObjects[0]);
        }

        [Test]
        public void TestSkippedLastObjectAddedInLastIteration()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    new TestHitObject { StartTime = 1 },
                    new TestHitObject
                    {
                        StartTime = 2,
                        Skip = true
                    },
                    new TestHitObject
                    {
                        StartTime = 3,
                        Skip = true
                    },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).CalculateTimed();

            Assert.That(attribs.Count, Is.EqualTo(1));
            assertEquals(attribs[0], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        private void assertEquals(TimedDifficultyAttributes attribs, params HitObject[] expected)
        {
            Assert.That(((TestDifficultyAttributes)attribs.Attributes).Objects, Is.EquivalentTo(expected));
        }

        private class TestHitObject : HitObject
        {
            /// <summary>
            /// Whether to skip generating a difficulty representation for this object.
            /// </summary>
            public bool Skip { get; set; }

            /// <summary>
            /// Whether to generate nested difficulty representations for this object, and if so, how many.
            /// </summary>
            public int Nested { get; set; }

            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                for (int i = 0; i < Nested; i++)
                    AddNested(new TestHitObject { StartTime = StartTime + 0.1 * i });
            }
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => Enumerable.Empty<Mod>();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => throw new NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new PassThroughBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new TestDifficultyCalculator(beatmap);

            public override string Description => string.Empty;
            public override string ShortName => string.Empty;

            private class PassThroughBeatmapConverter : IBeatmapConverter
            {
                public event Action<HitObject, IEnumerable<HitObject>>? ObjectConverted
                {
                    add { }
                    remove { }
                }

                public IBeatmap Beatmap { get; }

                public PassThroughBeatmapConverter(IBeatmap beatmap)
                {
                    Beatmap = beatmap;
                }

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default) => Beatmap;
            }
        }

        private class TestDifficultyCalculator : DifficultyCalculator
        {
            public TestDifficultyCalculator(IWorkingBeatmap beatmap)
                : base(new TestRuleset().RulesetInfo, beatmap)
            {
            }

            protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
                => new TestDifficultyAttributes { Objects = beatmap.HitObjects.ToArray() };

            protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
            {
                List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

                foreach (var obj in beatmap.HitObjects.OfType<TestHitObject>())
                {
                    if (!obj.Skip)
                        objects.Add(new DifficultyHitObject(obj, obj, clockRate, objects, objects.Count));

                    foreach (var nested in obj.NestedHitObjects)
                        objects.Add(new DifficultyHitObject(nested, nested, clockRate, objects, objects.Count));
                }

                return objects;
            }

            protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => new Skill[] { new PassThroughSkill(mods) };

            private class PassThroughSkill : Skill
            {
                public PassThroughSkill(Mod[] mods)
                    : base(mods)
                {
                }

                public override void Process(DifficultyHitObject current)
                {
                }

                public override double DifficultyValue() => 1;
            }
        }

        private class TestDifficultyAttributes : DifficultyAttributes
        {
            public HitObject[] Objects = Array.Empty<HitObject>();
        }
    }
}
