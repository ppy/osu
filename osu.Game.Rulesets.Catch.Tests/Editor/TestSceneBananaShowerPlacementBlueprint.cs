// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public class TestSceneBananaShowerPlacementBlueprint : CatchPlacementBlueprintTestScene
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableBananaShower((BananaShower)hitObject);

        protected override PlacementBlueprint CreateBlueprint() => new BananaShowerPlacementBlueprint();

        protected override void AddHitObject(DrawableHitObject hitObject)
        {
            // Create nested bananas (but positions are not randomized because beatmap processing is not done).
            hitObject.HitObject.ApplyDefaults(new ControlPointInfo(), Beatmap.Value.BeatmapInfo.BaseDifficulty);

            base.AddHitObject(hitObject);
        }
    }
}
