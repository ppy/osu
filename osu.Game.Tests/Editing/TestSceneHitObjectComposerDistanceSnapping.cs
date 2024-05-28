// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Tests.Editing
{
    [HeadlessTest]
    public partial class TestSceneHitObjectComposerDistanceSnapping : EditorClockTestScene
    {
        private TestHitObjectComposer composer = null!;

        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        protected override Container<Drawable> Content { get; } = new PopoverContainer { RelativeSizeAxes = Axes.Both };

        public TestSceneHitObjectComposerDistanceSnapping()
        {
            base.Content.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    editorBeatmap = new EditorBeatmap(new OsuBeatmap
                    {
                        BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo },
                    }),
                    Content
                },
            });
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                composer = new TestHitObjectComposer()
            };

            BeatDivisor.Value = 1;

            composer.EditorBeatmap.Difficulty.SliderMultiplier = 1;
            composer.EditorBeatmap.ControlPointInfo.Clear();
            composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });
        });

        [TestCase(1)]
        [TestCase(2)]
        public void TestSliderMultiplier(float multiplier)
        {
            AddStep($"set slider multiplier = {multiplier}", () => composer.EditorBeatmap.Difficulty.SliderMultiplier = multiplier);

            assertSnapDistance(100 * multiplier, null, true);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestSpeedMultiplierDoesNotChangeDistanceSnap(float multiplier)
        {
            assertSnapDistance(100, new Slider
            {
                SliderVelocityMultiplier = multiplier
            }, false);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestSpeedMultiplierDoesChangeDistanceSnap(float multiplier)
        {
            assertSnapDistance(100 * multiplier, new Slider
            {
                SliderVelocityMultiplier = multiplier
            }, true);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set divisor = {divisor}", () => BeatDivisor.Value = divisor);

            assertSnapDistance(100f / divisor, null, true);
        }

        /// <summary>
        /// The basic distance-duration functions should always include slider velocity of the reference object.
        /// </summary>
        [Test]
        public void TestConversionsWithSliderVelocity()
        {
            const float base_distance = 100;
            const float slider_velocity = 1.2f;

            var referenceObject = new Slider
            {
                SliderVelocityMultiplier = slider_velocity
            };

            assertSnapDistance(base_distance * slider_velocity, referenceObject, true);
            assertSnappedDistance(base_distance * slider_velocity + 10, base_distance * slider_velocity, referenceObject);
            assertSnappedDuration(base_distance * slider_velocity + 10, 1000, referenceObject);

            assertDistanceToDuration(base_distance * slider_velocity, 1000, referenceObject);
            assertDurationToDistance(1000, base_distance * slider_velocity, referenceObject);
        }

        [Test]
        public void TestConvertDurationToDistance()
        {
            assertDurationToDistance(500, 50);
            assertDurationToDistance(1000, 100);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.Difficulty.SliderMultiplier = 2);

            assertDurationToDistance(500, 100);
            assertDurationToDistance(1000, 200);

            AddStep("set beat length = 500", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            });

            assertDurationToDistance(500, 200);
            assertDurationToDistance(1000, 400);
        }

        [Test]
        public void TestConvertDistanceToDuration()
        {
            assertDistanceToDuration(50, 500);
            assertDistanceToDuration(100, 1000);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.Difficulty.SliderMultiplier = 2);

            assertDistanceToDuration(100, 500);
            assertDistanceToDuration(200, 1000);

            AddStep("set beat length = 500", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            });

            assertDistanceToDuration(200, 500);
            assertDistanceToDuration(400, 1000);
        }

        [Test]
        public void TestGetSnappedDurationFromDistance()
        {
            assertSnappedDuration(0, 0);
            assertSnappedDuration(50, 1000);
            assertSnappedDuration(100, 1000);
            assertSnappedDuration(150, 2000);
            assertSnappedDuration(200, 2000);
            assertSnappedDuration(250, 3000);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.Difficulty.SliderMultiplier = 2);

            assertSnappedDuration(0, 0);
            assertSnappedDuration(50, 0);
            assertSnappedDuration(100, 1000);
            assertSnappedDuration(150, 1000);
            assertSnappedDuration(200, 1000);
            assertSnappedDuration(250, 1000);

            AddStep("set beat length = 500", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            });

            assertSnappedDuration(50, 0);
            assertSnappedDuration(100, 500);
            assertSnappedDuration(150, 500);
            assertSnappedDuration(200, 500);
            assertSnappedDuration(250, 500);
            assertSnappedDuration(400, 1000);
        }

        [Test]
        public void GetSnappedDistanceFromDistance()
        {
            assertSnappedDistance(50, 0);
            assertSnappedDistance(100, 100);
            assertSnappedDistance(150, 100);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 200);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.Difficulty.SliderMultiplier = 2);

            assertSnappedDistance(50, 0);
            assertSnappedDistance(100, 0);
            assertSnappedDistance(150, 0);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 200);

            AddStep("set beat length = 500", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            });

            assertSnappedDistance(50, 0);
            assertSnappedDistance(100, 0);
            assertSnappedDistance(150, 0);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 200);
            assertSnappedDistance(400, 400);
        }

        [Test]
        public void TestUseCurrentSnap()
        {
            AddStep("add objects to beatmap", () =>
            {
                editorBeatmap.Add(new HitCircle { StartTime = 1000 });
                editorBeatmap.Add(new HitCircle { Position = new Vector2(100), StartTime = 2000 });
            });

            AddStep("hover use current snap button", () => InputManager.MoveMouseTo(composer.ChildrenOfType<ExpandableButton>().Single()));
            AddUntilStep("use current snap expanded", () => composer.ChildrenOfType<ExpandableButton>().Single().Expanded.Value, () => Is.True);

            AddStep("seek before first object", () => EditorClock.Seek(0));
            AddUntilStep("use current snap not available", () => composer.ChildrenOfType<ExpandableButton>().Single().Enabled.Value, () => Is.False);

            AddStep("seek to between objects", () => EditorClock.Seek(1500));
            AddUntilStep("use current snap available", () => composer.ChildrenOfType<ExpandableButton>().Single().Enabled.Value, () => Is.True);

            AddStep("seek after last object", () => EditorClock.Seek(2500));
            AddUntilStep("use current snap not available", () => composer.ChildrenOfType<ExpandableButton>().Single().Enabled.Value, () => Is.False);
        }

        private void assertSnapDistance(float expectedDistance, HitObject? referenceObject, bool includeSliderVelocity)
            => AddAssert($"distance is {expectedDistance}", () => composer.DistanceSnapProvider.GetBeatSnapDistanceAt(referenceObject ?? new HitObject(), includeSliderVelocity), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

        private void assertDurationToDistance(double duration, float expectedDistance, HitObject? referenceObject = null)
            => AddAssert($"duration = {duration} -> distance = {expectedDistance}", () => composer.DistanceSnapProvider.DurationToDistance(referenceObject ?? new HitObject(), duration), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

        private void assertDistanceToDuration(float distance, double expectedDuration, HitObject? referenceObject = null)
            => AddAssert($"distance = {distance} -> duration = {expectedDuration}", () => composer.DistanceSnapProvider.DistanceToDuration(referenceObject ?? new HitObject(), distance), () => Is.EqualTo(expectedDuration).Within(Precision.FLOAT_EPSILON));

        private void assertSnappedDuration(float distance, double expectedDuration, HitObject? referenceObject = null)
            => AddAssert($"distance = {distance} -> duration = {expectedDuration} (snapped)", () => composer.DistanceSnapProvider.FindSnappedDuration(referenceObject ?? new HitObject(), distance), () => Is.EqualTo(expectedDuration).Within(Precision.FLOAT_EPSILON));

        private void assertSnappedDistance(float distance, float expectedDistance, HitObject? referenceObject = null)
            => AddAssert($"distance = {distance} -> distance = {expectedDistance} (snapped)", () => composer.DistanceSnapProvider.FindSnappedDistance(referenceObject ?? new HitObject(), distance), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

        private partial class TestHitObjectComposer : OsuHitObjectComposer
        {
            public new EditorBeatmap EditorBeatmap => base.EditorBeatmap;

            public new IDistanceSnapProvider DistanceSnapProvider => base.DistanceSnapProvider;

            public TestHitObjectComposer()
                : base(new OsuRuleset())
            {
            }
        }
    }
}
