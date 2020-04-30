// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneHitCircleSelectionBlueprint : SelectionBlueprintTestScene
    {
        private HitCircle hitCircle;
        private DrawableHitCircle drawableObject;
        private TestBlueprint blueprint;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            hitCircle = new HitCircle { Position = new Vector2(256, 192) };
            hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableHitCircle(hitCircle));
            AddBlueprint(blueprint = new TestBlueprint(drawableObject));
        });

        [Test]
        public void TestInitialState()
        {
            AddAssert("blueprint positioned over hitobject", () => blueprint.CirclePiece.Position == hitCircle.Position);
        }

        [Test]
        public void TestMoveHitObject()
        {
            AddStep("move hitobject", () => hitCircle.Position = new Vector2(300, 225));
            AddAssert("blueprint positioned over hitobject", () => blueprint.CirclePiece.Position == hitCircle.Position);
        }

        [Test]
        public void TestMoveAfterApplyingDefaults()
        {
            AddStep("apply defaults", () => hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 }));
            AddStep("move hitobject", () => hitCircle.Position = new Vector2(300, 225));
            AddAssert("blueprint positioned over hitobject", () => blueprint.CirclePiece.Position == hitCircle.Position);
        }

        [Test]
        public void TestStackedHitObject()
        {
            AddStep("set stacking", () => hitCircle.StackHeight = 5);
            AddAssert("blueprint positioned over hitobject", () => blueprint.CirclePiece.Position == hitCircle.StackedPosition);
        }

        private class TestBlueprint : HitCircleSelectionBlueprint
        {
            public new HitCirclePiece CirclePiece => base.CirclePiece;

            public TestBlueprint(DrawableHitCircle drawableCircle)
                : base(drawableCircle)
            {
            }
        }
    }
}
