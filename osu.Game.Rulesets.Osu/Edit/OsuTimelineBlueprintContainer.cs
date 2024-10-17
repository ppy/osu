// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuTimelineBlueprintContainer : TimelineBlueprintContainer
    {
        public OsuTimelineBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        public override TimelineHitObjectBlueprint? CreateTimelineBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Spinner spinner:
                    return new SpinnerTimelineBlueprint(spinner);
            }

            return base.CreateTimelineBlueprintFor(hitObject);
        }
    }
}
