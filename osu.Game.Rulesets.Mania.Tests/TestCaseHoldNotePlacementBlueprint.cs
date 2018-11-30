// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestCaseHoldNotePlacementBlueprint : ManiaPlacementBlueprintTestCase
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableHoldNote((HoldNote)hitObject);
        protected override PlacementBlueprint CreateBlueprint() => new HoldNotePlacementBlueprint();
    }
}
