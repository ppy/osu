// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class TestSceneBananaShowerPlacementBlueprint : CatchPlacementBlueprintTestScene
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableBananaShower((BananaShower)hitObject);

        protected override PlacementBlueprint CreateBlueprint() => new BananaShowerPlacementBlueprint();

        protected override void AddHitObject(DrawableHitObject hitObject)
        {
            // Create nested bananas (but positions are not randomized because beatmap processing is not done).
            hitObject.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            base.AddHitObject(hitObject);
        }

        [Test]
        public void TestBasicPlacement()
        {
            const double start_time = 100;
            const double end_time = 500;

            AddMoveStep(start_time, 0);
            AddClickStep(MouseButton.Left);
            AddMoveStep(end_time, 0);
            AddClickStep(MouseButton.Right);
            AddAssert("banana shower is placed", () => LastObject is DrawableBananaShower);
            AddAssert("start time is correct", () => Precision.AlmostEquals(LastObject.HitObject.StartTime, start_time));
            AddAssert("end time is correct", () => Precision.AlmostEquals(LastObject.HitObject.GetEndTime(), end_time));
        }

        [Test]
        public void TestReversePlacement()
        {
            const double start_time = 100;
            const double end_time = 500;

            AddMoveStep(end_time, 0);
            AddClickStep(MouseButton.Left);

            AddMoveStep(start_time, 0);
            AddAssert("duration is positive", () => ((BananaShower)CurrentBlueprint.HitObject).Duration > 0);

            AddClickStep(MouseButton.Right);
            AddAssert("start time is correct", () => Precision.AlmostEquals(LastObject.HitObject.StartTime, start_time));
            AddAssert("end time is correct", () => Precision.AlmostEquals(LastObject.HitObject.GetEndTime(), end_time));
        }

        [Test]
        public void TestFinishWithZeroDuration()
        {
            AddMoveStep(100, 0);
            AddClickStep(MouseButton.Left);
            AddClickStep(MouseButton.Right);
            AddAssert("banana shower is not placed", () => LastObject == null);
            AddAssert("state is waiting", () => CurrentBlueprint?.PlacementActive == PlacementBlueprint.PlacementState.Waiting);
        }

        [Test]
        public void TestOpacity()
        {
            AddMoveStep(100, 0);
            AddClickStep(MouseButton.Left);
            AddUntilStep("outline is semitransparent", () => Precision.DefinitelyBigger(1, timeSpanOutline.Alpha));
            AddMoveStep(200, 0);
            AddUntilStep("outline is opaque", () => Precision.AlmostEquals(timeSpanOutline.Alpha, 1));
            AddMoveStep(100, 0);
            AddUntilStep("outline is semitransparent", () => Precision.DefinitelyBigger(1, timeSpanOutline.Alpha));
        }

        private TimeSpanOutline timeSpanOutline => Content.ChildrenOfType<TimeSpanOutline>().Single();
    }
}
