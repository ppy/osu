// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Desktop.Tests.Beatmaps;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Desktop.Tests.Visual
{
    /// <summary>
    /// The most minimal implementation of a playfield with scrolling hit objects.
    /// </summary>
    [TestFixture]
    public class TestCaseScrollingPlayfield : OsuTestCase
    {
        public TestCaseScrollingPlayfield()
        {
            Clock = new FramedClock();

            var objects = new List<HitObject>();

            int time = 1500;
            for (int i = 0; i < 50; i++)
            {
                objects.Add(new TestHitObject { StartTime = time });

                time += 500;
            }

            Beatmap b = new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata()
                }
            };

            WorkingBeatmap beatmap = new TestWorkingBeatmap(b);

            TestRulesetContainer horizontalRulesetContainer;
            Add(horizontalRulesetContainer = new TestRulesetContainer(Axes.X, beatmap, true));

            TestRulesetContainer verticalRulesetContainer;
            Add(verticalRulesetContainer = new TestRulesetContainer(Axes.Y, beatmap, true));

            AddStep("Reverse direction", () =>
            {
                horizontalRulesetContainer.Playfield.Reverse();
                verticalRulesetContainer.Playfield.Reverse();
            });
        }

        [Test]
        public void TestSpeedAdjustmentOrdering()
        {
            var hitObjectContainer = new ScrollingPlayfield.ScrollingHitObjectContainer(Axes.X);

            var speedAdjustments = new[]
            {
                new SpeedAdjustmentContainer(new MultiplierControlPoint()),
                new SpeedAdjustmentContainer(new MultiplierControlPoint(1000)
                {
                    TimingPoint = new TimingControlPoint { BeatLength = 500 }
                }),
                new SpeedAdjustmentContainer(new MultiplierControlPoint(2000)
                {
                    TimingPoint = new TimingControlPoint { BeatLength = 1000 },
                    DifficultyPoint = new DifficultyControlPoint { SpeedMultiplier = 2}
                }),
                new SpeedAdjustmentContainer(new MultiplierControlPoint(3000)
                {
                    TimingPoint = new TimingControlPoint { BeatLength = 1000 },
                    DifficultyPoint = new DifficultyControlPoint { SpeedMultiplier = 1}
                }),
            };

            var hitObjects = new[]
            {
                new DrawableTestHitObject(Axes.X, new TestHitObject { StartTime = -1000 }),
                new DrawableTestHitObject(Axes.X, new TestHitObject()),
                new DrawableTestHitObject(Axes.X, new TestHitObject { StartTime = 1000 }),
                new DrawableTestHitObject(Axes.X, new TestHitObject { StartTime = 2000 }),
                new DrawableTestHitObject(Axes.X, new TestHitObject { StartTime = 3000 }),
                new DrawableTestHitObject(Axes.X, new TestHitObject { StartTime = 4000 }),
            };

            hitObjects.ForEach(h => hitObjectContainer.Add(h));
            speedAdjustments.ForEach(hitObjectContainer.AddSpeedAdjustment);

            // The 0th index in hitObjectContainer.SpeedAdjustments is the "default" control point
            // Check multiplier of the default speed adjustment
            Assert.AreEqual(1, hitObjectContainer.SpeedAdjustments[0].ControlPoint.Multiplier);
            Assert.AreEqual(1, speedAdjustments[0].ControlPoint.Multiplier);
            Assert.AreEqual(2, speedAdjustments[1].ControlPoint.Multiplier);
            Assert.AreEqual(2, speedAdjustments[2].ControlPoint.Multiplier);
            Assert.AreEqual(1, speedAdjustments[3].ControlPoint.Multiplier);

            // Check insertion of hit objects
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[4].Contains(hitObjects[0]));
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[3].Contains(hitObjects[1]));
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[2].Contains(hitObjects[2]));
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[1].Contains(hitObjects[3]));
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[0].Contains(hitObjects[4]));
            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[0].Contains(hitObjects[5]));

            hitObjectContainer.RemoveSpeedAdjustment(hitObjectContainer.SpeedAdjustments[3]);

            // The hit object contained in this speed adjustment should be resorted into the one occuring before it

            Assert.IsTrue(hitObjectContainer.SpeedAdjustments[3].Contains(hitObjects[1]));
        }

        private class TestRulesetContainer : ScrollingRulesetContainer<TestPlayfield, TestHitObject>
        {
            private readonly Axes scrollingAxes;

            public TestRulesetContainer(Axes scrollingAxes, WorkingBeatmap beatmap, bool isForCurrentRuleset)
                : base(null, beatmap, isForCurrentRuleset)
            {
                this.scrollingAxes = scrollingAxes;
            }

            public new TestPlayfield Playfield => base.Playfield;

            public override ScoreProcessor CreateScoreProcessor() => new TestScoreProcessor();

            public override PassThroughInputManager CreateInputManager() => new PassThroughInputManager();

            protected override BeatmapConverter<TestHitObject> CreateBeatmapConverter() => new TestBeatmapConverter();

            protected override Playfield CreatePlayfield() => new TestPlayfield(scrollingAxes);

            protected override DrawableHitObject<TestHitObject> GetVisualRepresentation(TestHitObject h) => new DrawableTestHitObject(scrollingAxes, h);
        }

        private class TestScoreProcessor : ScoreProcessor<TestHitObject>
        {
            protected override void OnNewJudgement(Judgement judgement)
            {
            }
        }

        private class TestBeatmapConverter : BeatmapConverter<TestHitObject>
        {
            protected override IEnumerable<Type> ValidConversionTypes => new[] { typeof(HitObject) };

            protected override IEnumerable<TestHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
            {
                yield return original as TestHitObject;
            }
        }

        private class DrawableTestHitObject : DrawableScrollingHitObject<TestHitObject>
        {
            public DrawableTestHitObject(Axes scrollingAxes, TestHitObject hitObject)
                : base(hitObject)
            {
                Anchor = scrollingAxes == Axes.Y ? Anchor.TopCentre : Anchor.CentreLeft;
                Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;

                Add(new Circle
                {
                    Size = new Vector2(50)
                });
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }

        private class TestPlayfield : ScrollingPlayfield
        {
            protected override Container<Drawable> Content => content;
            private readonly Container<Drawable> content;

            public TestPlayfield(Axes scrollingAxes)
                : base(scrollingAxes)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.2f
                    },
                    content = new Container { RelativeSizeAxes = Axes.Both }
                };
            }

            public void Reverse() => Reversed.Toggle();
        }


        private class TestHitObject : HitObject
        {
        }
    }
}
