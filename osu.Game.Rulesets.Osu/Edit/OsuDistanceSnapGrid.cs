// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuDistanceSnapGrid : CircularDistanceSnapGrid
    {
        public OsuDistanceSnapGrid(OsuHitObject hitObject, [CanBeNull] OsuHitObject nextHitObject = null)
            : base(hitObject, hitObject.StackedEndPosition, hitObject.GetEndTime(), nextHitObject?.StartTime)
        {
            Masking = true;
        }
    }
}
