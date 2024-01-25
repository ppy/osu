// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchDistanceSnapProvider : ComposerDistanceSnapProvider
    {
        protected override double ReadCurrentDistanceSnap(HitObject before, HitObject after)
        {
            // osu!catch's distance snap implementation is limited, in that a custom spacing cannot be specified.
            // Therefore this functionality is not currently used.
            //
            // The implementation below is probably correct but should be checked if/when exposed via controls.

            float expectedDistance = DurationToDistance(before, after.StartTime - before.GetEndTime());
            float actualDistance = Math.Abs(((CatchHitObject)before).EffectiveX - ((CatchHitObject)after).EffectiveX);

            return actualDistance / expectedDistance;
        }
    }
}
