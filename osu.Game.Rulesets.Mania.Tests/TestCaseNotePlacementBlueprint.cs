// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestCaseNotePlacementBlueprint : ManiaPlacementBlueprintTestCase
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableNote((Note)hitObject);
        protected override PlacementBlueprint CreateBlueprint() => new NotePlacementBlueprint();
    }
}
