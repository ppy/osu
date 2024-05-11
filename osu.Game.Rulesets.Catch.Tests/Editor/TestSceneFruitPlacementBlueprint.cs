// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class TestSceneFruitPlacementBlueprint : CatchPlacementBlueprintTestScene
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableFruit((Fruit)hitObject);

        protected override PlacementBlueprint CreateBlueprint() => new FruitPlacementBlueprint();

        [Test]
        public void TestFruitPlacementPosition()
        {
            const double time = 300;
            const float x = CatchPlayfield.CENTER_X;

            AddMoveStep(time, x);
            AddClickStep(MouseButton.Left);

            AddAssert("outline position is correct", () =>
            {
                var outline = FruitOutlines.Single();
                return Precision.AlmostEquals(outline.X, x) &&
                       Precision.AlmostEquals(outline.Y, HitObjectContainer.PositionAtTime(time));
            });

            AddAssert("fruit time is correct", () => Precision.AlmostEquals(LastObject.StartTimeBindable.Value, time));
            AddAssert("fruit position is correct", () => Precision.AlmostEquals(LastObject.X, x));
        }
    }
}
