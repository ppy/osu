// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSpinnerPlacementBlueprint : PlacementBlueprintTestScene
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableSpinner((Spinner)hitObject);

        protected override PlacementBlueprint CreateBlueprint() => new SpinnerPlacementBlueprint();
    }
}
