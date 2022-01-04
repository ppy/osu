// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Utils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;
using JetBrains.Annotations;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneDrawableScrollingRuleset : OsuTestScene
    {
        /// <summary>
        /// The amount of time visible by the "view window" of the playfield.
        /// All hitobjects added through <see cref="createBeatmap"/> are spaced apart by this value, such that for a beat length of 1000,
        /// there will be at most 2 hitobjects visible in the "view window".
        /// </summary>
        private const double time_range = 1000;

        private readonly ManualClock testClock = new ManualClock();
        private TestDrawableScrollingRuleset drawableRuleset;

        [SetUp]
        public void Setup() => Schedule(() => testClock.CurrentTime = 0);

        [TestCase("pooled")]
        [TestCase("non-pooled")]
        public void TestHitObjectLifetime(string pooled)
        {
            var beatmap = createBeatmap(_ => pooled == "pooled" ? new TestPooledHitObject() : new TestHitObject());
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            createTest(beatmap);

            assertPosition(0, 0f);
            assertDead(3);

            setTime(3 * time_range);
            assertPosition(3, 0f);
            assertDead(0);

            setTime(0 * time_range);
            assertPosition(0, 0f);
            assertDead(3);
        }

        [TestCase("pooled")]
        [TestCase("non-pooled")]
        public void TestNestedHitObject(string pooled)
        {
            var beatmap = createBeatmap(i =>
            {
                var h = pooled == "pooled" ? new TestPooledParentHitObject() : new TestParentHitObject();
                h.Duration = 300;
                h.ChildTimeOffset = i % 3 * 100;
                return h;
            });
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            createTest(beatmap);

            assertPosition(0, 0f);
            assertHeight(0);
            assertChildPosition(0);

            setTime(5 * time_range);
            assertPosition(5, 0f);
            assertHeight(5);
            assertChildPosition(5);
        }

        [TestCase("pooled")]
        [TestCase("non-pooled")]
        public void TestLifetimeRecomputedWhenTimeRangeChanges(string pooled)
        {
            var beatmap = createBeatmap(_ => pooled == "pooled" ? new TestPooledHitObject() : new TestHitObject());
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            createTest(beatmap);

            assertDead(3);

            AddStep("increase time range", () => drawableRuleset.TimeRange.Value = 3 * time_range);
            assertPosition(3, 1);
        }

        [Test]
        public void TestRelativeBeatLengthScaleSingleTimingPoint()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            assertPosition(0, 0f);

            // The single timing point is 1x speed relative to itself, such that the hitobject occurring time_range milliseconds later should appear
            // at the bottom of the view window regardless of the timing point's beat length
            assertPosition(1, 1f);
        }

        [Test]
        public void TestRelativeBeatLengthScaleTimingPointBeyondEndDoesNotBecomeDominant()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range / 2 });
            beatmap.ControlPointInfo.Add(12000, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(100000, new TimingControlPoint { BeatLength = time_range });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            assertPosition(0, 0f);
            assertPosition(1, 1f);
        }

        [Test]
        public void TestRelativeBeatLengthScaleFromSecondTimingPoint()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(3 * time_range, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            // The first timing point should have a relative velocity of 2
            assertPosition(0, 0f);
            assertPosition(1, 0.5f);
            assertPosition(2, 1f);

            // Move to the second timing point
            setTime(3 * time_range);
            assertPosition(3, 0f);

            // As above, this is the timing point that is 1x speed relative to itself, so the hitobject occurring time_range milliseconds later should be at the bottom of the view window
            assertPosition(4, 1f);
        }

        [Test]
        public void TestNonRelativeScale()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(3 * time_range, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap);

            assertPosition(0, 0f);
            assertPosition(1, 1);

            // Move to the second timing point
            setTime(3 * time_range);
            assertPosition(3, 0f);

            // For a beat length of 500, the view window of this timing point is elongated 2x (1000 / 500), such that the second hitobject is two TimeRanges away (offscreen)
            // To bring it on-screen, half TimeRange is added to the current time, bringing the second half of the view window into view, and the hitobject should appear at the bottom
            setTime(3 * time_range + time_range / 2);
            assertPosition(4, 1f);
        }

        [Test]
        public void TestSliderMultiplierDoesNotAffectRelativeBeatLength()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.Difficulty.SliderMultiplier = 2;

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);
            AddStep("adjust time range", () => drawableRuleset.TimeRange.Value = 5000);

            for (int i = 0; i < 5; i++)
                assertPosition(i, i / 5f);
        }

        [Test]
        public void TestSliderMultiplierAffectsNonRelativeBeatLength()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.Difficulty.SliderMultiplier = 2;

            createTest(beatmap);
            AddStep("adjust time range", () => drawableRuleset.TimeRange.Value = 2000);

            assertPosition(0, 0);
            assertPosition(1, 1);
        }

        /// <summary>
        /// Get a <see cref="DrawableTestHitObject" /> corresponding to the <paramref name="index"/>'th <see cref="TestHitObject"/>.
        /// When the hit object is not alive, `null` is returned.
        /// </summary>
        [CanBeNull]
        private DrawableTestHitObject getDrawableHitObject(int index)
        {
            var hitObject = drawableRuleset.Beatmap.HitObjects.ElementAt(index);
            return (DrawableTestHitObject)drawableRuleset.Playfield.HitObjectContainer.AliveObjects.FirstOrDefault(obj => obj.HitObject == hitObject);
        }

        private float yScale => drawableRuleset.Playfield.HitObjectContainer.DrawHeight;

        private void assertDead(int index) => AddAssert($"hitobject {index} is dead", () => getDrawableHitObject(index) == null);

        private void assertHeight(int index) => AddAssert($"hitobject {index} height", () =>
        {
            var d = getDrawableHitObject(index);
            return d != null && Precision.AlmostEquals(d.DrawHeight, yScale * (float)(d.HitObject.Duration / time_range), 0.1f);
        });

        private void assertChildPosition(int index) => AddAssert($"hitobject {index} child position", () =>
        {
            var d = getDrawableHitObject(index);
            return d is DrawableTestParentHitObject && Precision.AlmostEquals(
                d.NestedHitObjects.First().DrawPosition.Y,
                yScale * (float)((TestParentHitObject)d.HitObject).ChildTimeOffset / time_range, 0.1f);
        });

        private void assertPosition(int index, float relativeY) => AddAssert($"hitobject {index} at {relativeY}",
            () => Precision.AlmostEquals(getDrawableHitObject(index)?.DrawPosition.Y ?? -1, yScale * relativeY));

        private void setTime(double time)
        {
            AddStep($"set time = {time}", () => testClock.CurrentTime = time);
        }

        /// <summary>
        /// Creates an <see cref="IBeatmap"/>, containing 10 hitobjects and user-provided timing points.
        /// The hitobjects are spaced <see cref="time_range"/> milliseconds apart.
        /// </summary>
        /// <returns>The <see cref="IBeatmap"/>.</returns>
        private IBeatmap createBeatmap(Func<int, TestHitObject> createAction = null)
        {
            var beatmap = new Beatmap<TestHitObject> { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } };

            for (int i = 0; i < 10; i++)
            {
                var h = createAction?.Invoke(i) ?? new TestHitObject();
                h.StartTime = i * time_range;
                beatmap.HitObjects.Add(h);
            }

            return beatmap;
        }

        private void createTest(IBeatmap beatmap, Action<TestDrawableScrollingRuleset> overrideAction = null) => AddStep("create test", () =>
        {
            var ruleset = new TestScrollingRuleset();

            drawableRuleset = (TestDrawableScrollingRuleset)ruleset.CreateDrawableRulesetWith(CreateWorkingBeatmap(beatmap).GetPlayableBeatmap(ruleset.RulesetInfo));
            drawableRuleset.FrameStablePlayback = false;

            overrideAction?.Invoke(drawableRuleset);

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Height = 0.75f,
                Width = 400,
                Masking = true,
                Clock = new FramedClock(testClock),
                Child = drawableRuleset
            };
        });

        #region Ruleset

        private class TestScrollingRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new TestDrawableScrollingRuleset(this, beatmap, mods);

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TestBeatmapConverter(beatmap, null);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description { get; } = string.Empty;

            public override string ShortName { get; } = string.Empty;
        }

        private class TestDrawableScrollingRuleset : DrawableScrollingRuleset<TestHitObject>
        {
            public bool RelativeScaleBeatLengthsOverride { get; set; }

            protected override bool RelativeScaleBeatLengths => RelativeScaleBeatLengthsOverride;

            protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

            public new Bindable<double> TimeRange => base.TimeRange;

            public TestDrawableScrollingRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
                : base(ruleset, beatmap, mods)
            {
                TimeRange.Value = time_range;
            }

            public override DrawableHitObject<TestHitObject> CreateDrawableRepresentation(TestHitObject h)
            {
                switch (h)
                {
                    case TestPooledHitObject _:
                    case TestPooledParentHitObject _:
                        return null;

                    case TestParentHitObject p:
                        return new DrawableTestParentHitObject(p);

                    default:
                        return new DrawableTestHitObject(h);
                }
            }

            protected override PassThroughInputManager CreateInputManager() => new PassThroughInputManager();

            protected override Playfield CreatePlayfield() => new TestPlayfield();
        }

        private class TestPlayfield : ScrollingPlayfield
        {
            public TestPlayfield()
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 150 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2,
                                    Colour = Color4.Green
                                },
                                HitObjectContainer
                            }
                        }
                    }
                });

                RegisterPool<TestPooledHitObject, DrawableTestPooledHitObject>(1);
                RegisterPool<TestPooledParentHitObject, DrawableTestPooledParentHitObject>(1);
            }
        }

        private class TestBeatmapConverter : BeatmapConverter<TestHitObject>
        {
            public TestBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
                : base(beatmap, ruleset)
            {
            }

            public override bool CanConvert() => true;

            protected override IEnumerable<TestHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken) =>
                throw new NotImplementedException();
        }

        #endregion

        #region HitObject

        private class TestHitObject : HitObject, IHasDuration
        {
            public double EndTime => StartTime + Duration;

            public double Duration { get; set; } = 100;
        }

        private class TestPooledHitObject : TestHitObject
        {
        }

        private class TestParentHitObject : TestHitObject
        {
            public double ChildTimeOffset;

            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                AddNested(new TestHitObject { StartTime = StartTime + ChildTimeOffset });
            }
        }

        private class TestPooledParentHitObject : TestParentHitObject
        {
            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                AddNested(new TestPooledHitObject { StartTime = StartTime + ChildTimeOffset });
            }
        }

        private class DrawableTestHitObject : DrawableHitObject<TestHitObject>
        {
            public DrawableTestHitObject([CanBeNull] TestHitObject hitObject)
                : base(hitObject)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;

                Size = new Vector2(100, 25);

                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.LightPink
                    },
                    new Box
                    {
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = Color4.Red
                    }
                });
            }

            protected override void Update() => LifetimeEnd = HitObject.EndTime;
        }

        private class DrawableTestPooledHitObject : DrawableTestHitObject
        {
            public DrawableTestPooledHitObject()
                : base(null)
            {
                InternalChildren[0].Colour = Color4.LightSkyBlue;
                InternalChildren[1].Colour = Color4.Blue;
            }
        }

        private class DrawableTestParentHitObject : DrawableTestHitObject
        {
            private readonly Container<DrawableHitObject> container;

            public DrawableTestParentHitObject([CanBeNull] TestHitObject hitObject)
                : base(hitObject)
            {
                InternalChildren[0].Colour = Color4.LightYellow;
                InternalChildren[1].Colour = Color4.Yellow;

                AddInternal(container = new Container<DrawableHitObject>
                {
                    RelativeSizeAxes = Axes.Both,
                });
            }

            protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject) =>
                new DrawableTestHitObject((TestHitObject)hitObject);

            protected override void AddNestedHitObject(DrawableHitObject hitObject) => container.Add(hitObject);

            protected override void ClearNestedHitObjects() => container.Clear(false);
        }

        private class DrawableTestPooledParentHitObject : DrawableTestParentHitObject
        {
            public DrawableTestPooledParentHitObject()
                : base(null)
            {
                InternalChildren[0].Colour = Color4.LightSeaGreen;
                InternalChildren[1].Colour = Color4.Green;
            }
        }

        #endregion
    }
}
