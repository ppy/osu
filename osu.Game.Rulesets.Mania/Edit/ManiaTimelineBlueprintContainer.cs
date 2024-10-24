// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class ManiaTimelineBlueprintContainer : TimelineBlueprintContainer
    {
        public ManiaTimelineBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        public override TimelineHitObjectBlueprint? CreateTimelineBlueprintFor(HitObject hitObject)
        {
            return null;
        }
    }
}
