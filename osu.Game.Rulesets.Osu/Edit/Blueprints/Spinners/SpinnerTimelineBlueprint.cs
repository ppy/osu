// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners
{
    public partial class SpinnerTimelineBlueprint : TimelineHitObjectBlueprint
    {
        public SpinnerTimelineBlueprint(HitObject item)
            : base(item)
        {
            SamplePointPiece.X = 1;
        }
    }
}
