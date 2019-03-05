// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseHitCircleSelectionBlueprint : SelectionBlueprintTestCase
    {
        private readonly DrawableHitCircle drawableObject;

        public TestCaseHitCircleSelectionBlueprint()
        {
            var hitCircle = new HitCircle { Position = new Vector2(256, 192) };
            hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            Add(drawableObject = new DrawableHitCircle(hitCircle));
        }

        protected override SelectionBlueprint CreateBlueprint() => new HitCircleSelectionBlueprint(drawableObject);
    }
}
