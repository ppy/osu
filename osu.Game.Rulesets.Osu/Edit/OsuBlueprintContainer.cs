// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuBlueprintContainer : ComposeBlueprintContainer
    {
        public OsuBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new OsuSelectionHandler();

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case HitCircle circle:
                    return new HitCircleSelectionBlueprint(circle);

                case Slider slider:
                    return new SliderSelectionBlueprint(slider);

                case Spinner spinner:
                    return new SpinnerSelectionBlueprint(spinner);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }
    }
}
