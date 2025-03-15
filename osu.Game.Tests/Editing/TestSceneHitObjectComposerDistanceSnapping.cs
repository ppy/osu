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
using osu.Game.Rulesets.Objects.Types;
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
            Child = composer = new TestHitObjectComposer();

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

            assertSnapDistance(100 * multiplier);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestSpeedMultiplierDoesChangeDistanceSnap(float multiplier)
        {
            assertSnapDistance(100 * multiplier, new Slider
            {
                SliderVelocityMultiplier = multiplier
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set divisor = {divisor}", () => BeatDivisor.Value = divisor);

            assertSnapDistance(100f / divisor);
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
            AddStep("add to beatmap", () => composer.EditorBeatmap.Add(referenceObject));

            assertSnapDistance(base_distance * slider_velocity, referenceObject);
            assertSnappedDistance(base_distance * slider_velocity + 10, base_distance * slider_velocity, referenceObject);

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
        public void TestUnsnappedObject()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        // simulate object snapped to 1/3rds
                        // this object's end time will be 2000 / 3 = 666.66... ms
                        new PathControlPoint(new Vector2(200 / 3f, 0)),
                    }
                }
            };

            AddStep("add slider", () => composer.EditorBeatmap.Add(slider));
            AddStep("set snap to 1/4", () => BeatDivisor.Value = 4);

            // with default beat length of 1000ms and snap at 1/4, the valid snap times are 500ms, 750ms, and 1000ms
            // with default settings, the snapped distance will be a tenth of the difference of the time delta

            // (500 - 666.66...) / 10 = -16.66... = -100 / 6
            assertSnappedDistance(0, -100 / 6f, slider);
            assertSnappedDistance(7, -100 / 6f, slider);

            // (750 - 666.66...) / 10 = 8.33... = 100 / 12
            assertSnappedDistance(9, 100 / 12f, slider);
            assertSnappedDistance(33, 100 / 12f, slider);

            // (1000 - 666.66...) / 10 = 33.33... = 100 / 3
            assertSnappedDistance(34, 100 / 3f, slider);
        }

        [Test]
        public void TestUseCurrentSnap()
        {
            ExpandableButton getCurrentSnapButton() => composer.ChildrenOfType<EditorToolboxGroup>().Single(g => g.Name == "snapping")
                                                               .ChildrenOfType<ExpandableButton>().Single();

            AddStep("add objects to beatmap", () =>
            {
                editorBeatmap.Add(new HitCircle { StartTime = 1000 });
                editorBeatmap.Add(new HitCircle { Position = new Vector2(100), StartTime = 2000 });
            });

            AddStep("hover use current snap button", () => InputManager.MoveMouseTo(getCurrentSnapButton()));
            AddUntilStep("use current snap expanded", () => getCurrentSnapButton().Expanded.Value, () => Is.True);

            AddStep("seek before first object", () => EditorClock.Seek(0));
            AddUntilStep("use current snap not available", () => getCurrentSnapButton().Enabled.Value, () => Is.False);

            AddStep("seek to between objects", () => EditorClock.Seek(1500));
            AddUntilStep("use current snap available", () => getCurrentSnapButton().Enabled.Value, () => Is.True);

            AddStep("seek after last object", () => EditorClock.Seek(2500));
            AddUntilStep("use current snap not available", () => getCurrentSnapButton().Enabled.Value, () => Is.False);
        }

        private void assertSnapDistance(float expectedDistance, IHasSliderVelocity? hasSliderVelocity = null)
            => AddAssert($"distance is {expectedDistance}", () => composer.DistanceSnapProvider.GetBeatSnapDistance(hasSliderVelocity), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

        private void assertDurationToDistance(double duration, float expectedDistance, HitObject? referenceObject = null)
            => AddAssert($"duration = {duration} -> distance = {expectedDistance}", () => composer.DistanceSnapProvider.DurationToDistance(duration, referenceObject?.StartTime ?? 0, referenceObject as IHasSliderVelocity), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

        private void assertDistanceToDuration(float distance, double expectedDuration, HitObject? referenceObject = null)
            => AddAssert($"distance = {distance} -> duration = {expectedDuration}", () => composer.DistanceSnapProvider.DistanceToDuration(distance, referenceObject?.StartTime ?? 0, referenceObject as IHasSliderVelocity), () => Is.EqualTo(expectedDuration).Within(Precision.FLOAT_EPSILON));

        private void assertSnappedDistance(float distance, float expectedDistance, HitObject? referenceObject = null)
            => AddAssert($"distance = {distance} -> distance = {expectedDistance} (snapped)", () => composer.DistanceSnapProvider.FindSnappedDistance(distance, referenceObject?.GetEndTime() ?? 0, referenceObject as IHasSliderVelocity), () => Is.EqualTo(expectedDistance).Within(Precision.FLOAT_EPSILON));

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
