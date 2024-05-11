// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class TestSceneJuiceStreamPlacementBlueprint : CatchPlacementBlueprintTestScene
    {
        private const double velocity_factor = 0.5;

        private JuiceStream lastObject => LastObject?.HitObject as JuiceStream;

        protected override IBeatmap GetPlayableBeatmap()
        {
            var playable = base.GetPlayableBeatmap();
            playable.Difficulty.SliderTickRate = 5;
            playable.Difficulty.SliderMultiplier = velocity_factor * 10;
            return playable;
        }

        [Test]
        public void TestBasicPlacement()
        {
            double[] times = { 300, 800 };
            float[] positions = { 100, 200 };
            addPlacementSteps(times, positions);

            AddAssert("juice stream is placed", () => lastObject != null);
            AddAssert("start time is correct", () => Precision.AlmostEquals(lastObject.StartTime, times[0]));
            AddAssert("end time is correct", () => Precision.AlmostEquals(lastObject.EndTime, times[1]));
            AddAssert("start position is correct", () => Precision.AlmostEquals(lastObject.OriginalX, positions[0]));
            AddAssert("end position is correct", () => Precision.AlmostEquals(lastObject.EndX, positions[1]));
            AddAssert("default slider velocity", () => lastObject.SliderVelocityMultiplierBindable.IsDefault);
        }

        [Test]
        public void TestEmptyNotCommitted()
        {
            addMoveAndClickSteps(100, 100);
            addMoveAndClickSteps(100, 100);
            addMoveAndClickSteps(100, 100, true);
            AddAssert("juice stream not placed", () => lastObject == null);
        }

        [Test]
        public void TestMultipleSegments()
        {
            double[] times = { 100, 300, 500, 700 };
            float[] positions = { 100, 150, 100, 100 };
            addPlacementSteps(times, positions);

            AddAssert("has 4 vertices", () => lastObject.Path.ControlPoints.Count == 4);
            addPathCheckStep(times, positions);
        }

        [Test]
        public void TestSliderVelocityChange()
        {
            double[] times = { 100, 300 };
            float[] positions = { 200, 500 };
            addPlacementSteps(times, positions);
            addPathCheckStep(times, positions);

            AddAssert("slider velocity changed", () => !lastObject.SliderVelocityMultiplierBindable.IsDefault);
        }

        [Test]
        public void TestClampedPositionIsRestored()
        {
            double[] times = { 100, 300, 500 };
            float[] positions = { 200, 200, -3000, 250 };

            addMoveAndClickSteps(times[0], positions[0]);
            addMoveAndClickSteps(times[1], positions[1]);
            AddMoveStep(times[2], positions[2]);
            addMoveAndClickSteps(times[2], positions[3], true);

            addPathCheckStep(times, new float[] { 200, 200, 250 });
        }

        [Test]
        public void TestOutOfOrder()
        {
            double[] times = { 100, 700, 500, 300 };
            float[] positions = { 100, 200, 150, 50 };
            addPlacementSteps(times, positions);
            addPathCheckStep(times, positions);
        }

        [Test]
        public void TestMoveBeforeFirstVertex()
        {
            double[] times = { 300, 500, 100 };
            float[] positions = { 100, 100, 100 };
            addPlacementSteps(times, positions);
            AddAssert("start time is correct", () => Precision.AlmostEquals(lastObject.StartTime, times[0]));
            AddAssert("end time is correct", () => Precision.AlmostEquals(lastObject.EndTime, times[1], 1e-3));
        }

        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableJuiceStream((JuiceStream)hitObject);

        protected override PlacementBlueprint CreateBlueprint() => new JuiceStreamPlacementBlueprint();

        private void addMoveAndClickSteps(double time, float position, bool end = false)
        {
            AddMoveStep(time, position);
            AddClickStep(end ? MouseButton.Right : MouseButton.Left);
        }

        private void addPlacementSteps(double[] times, float[] positions)
        {
            for (int i = 0; i < times.Length; i++)
                addMoveAndClickSteps(times[i], positions[i], i == times.Length - 1);
        }

        private void addPathCheckStep(double[] times, float[] positions) => AddStep("assert path is correct", () =>
            Assert.That(getPositions(times), Is.EqualTo(positions).Within(Precision.FLOAT_EPSILON)));

        private float[] getPositions(IEnumerable<double> times)
        {
            JuiceStream hitObject = lastObject.AsNonNull();
            return times
                   .Select(time => (time - hitObject.StartTime) * hitObject.Velocity)
                   .Select(distance => hitObject.EffectiveX + hitObject.Path.PositionAt(distance / hitObject.Distance).X)
                   .ToArray();
        }
    }
}
