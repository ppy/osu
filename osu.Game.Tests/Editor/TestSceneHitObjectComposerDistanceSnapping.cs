// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Editor
{
    [HeadlessTest]
    public class TestSceneHitObjectComposerDistanceSnapping : EditorClockTestScene
    {
        private TestHitObjectComposer composer;

        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        protected override Container<Drawable> Content { get; }

        public TestSceneHitObjectComposerDistanceSnapping()
        {
            base.Content.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    editorBeatmap = new EditorBeatmap(new OsuBeatmap()),
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
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

            composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 1;
            composer.EditorBeatmap.ControlPointInfo.Clear();

            composer.EditorBeatmap.ControlPointInfo.Add(0, new DifficultyControlPoint { SpeedMultiplier = 1 });
            composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });
        });

        [TestCase(1)]
        [TestCase(2)]
        public void TestSliderMultiplier(float multiplier)
        {
            AddStep($"set multiplier = {multiplier}", () => composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = multiplier);

            assertSnapDistance(100 * multiplier);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestSpeedMultiplier(float multiplier)
        {
            AddStep($"set multiplier = {multiplier}", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new DifficultyControlPoint { SpeedMultiplier = multiplier });
            });

            assertSnapDistance(100 * multiplier);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set divisor = {divisor}", () => BeatDivisor.Value = divisor);

            assertSnapDistance(100f / divisor);
        }

        [Test]
        public void TestConvertDurationToDistance()
        {
            assertDurationToDistance(500, 50);
            assertDurationToDistance(1000, 100);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2);

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

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2);

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

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2);

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
            assertSnappedDistance(50, 100);
            assertSnappedDistance(100, 100);
            assertSnappedDistance(150, 200);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 300);

            AddStep("set slider multiplier = 2", () => composer.EditorBeatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2);

            assertSnappedDistance(50, 0);
            assertSnappedDistance(100, 200);
            assertSnappedDistance(150, 200);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 200);

            AddStep("set beat length = 500", () =>
            {
                composer.EditorBeatmap.ControlPointInfo.Clear();
                composer.EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            });

            assertSnappedDistance(50, 0);
            assertSnappedDistance(100, 200);
            assertSnappedDistance(150, 200);
            assertSnappedDistance(200, 200);
            assertSnappedDistance(250, 200);
            assertSnappedDistance(400, 400);
        }

        private void assertSnapDistance(float expectedDistance)
            => AddAssert($"distance is {expectedDistance}", () => composer.GetBeatSnapDistanceAt(0) == expectedDistance);

        private void assertDurationToDistance(double duration, float expectedDistance)
            => AddAssert($"duration = {duration} -> distance = {expectedDistance}", () => composer.DurationToDistance(0, duration) == expectedDistance);

        private void assertDistanceToDuration(float distance, double expectedDuration)
            => AddAssert($"distance = {distance} -> duration = {expectedDuration}", () => composer.DistanceToDuration(0, distance) == expectedDuration);

        private void assertSnappedDuration(float distance, double expectedDuration)
            => AddAssert($"distance = {distance} -> duration = {expectedDuration} (snapped)", () => composer.GetSnappedDurationFromDistance(0, distance) == expectedDuration);

        private void assertSnappedDistance(float distance, float expectedDistance)
            => AddAssert($"distance = {distance} -> distance = {expectedDistance} (snapped)", () => composer.GetSnappedDistanceFromDistance(0, distance) == expectedDistance);

        private class TestHitObjectComposer : OsuHitObjectComposer
        {
            public new EditorBeatmap EditorBeatmap => base.EditorBeatmap;

            public TestHitObjectComposer()
                : base(new OsuRuleset())
            {
            }
        }
    }
}
