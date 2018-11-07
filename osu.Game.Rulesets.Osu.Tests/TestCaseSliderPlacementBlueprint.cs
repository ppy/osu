// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderPlacementBlueprint : PlacementBlueprintTestCase
    {
        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableSlider((Slider)hitObject);
        protected override PlacementBlueprint CreateBlueprint() => new SliderPlacementBlueprint();
    }
}
