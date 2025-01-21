// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// Legacy "Hold" hit object type. Generally only valid in the mania ruleset.
    /// </summary>
    /// <remarks>
    /// Only used for parsing beatmaps and not gameplay.
    /// </remarks>
    internal sealed class ConvertHold : ConvertHitObject, IHasDuration
    {
        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;

        public ConvertHold()
        {
            LegacyType = LegacyHitObjectType.Hold;
        }
    }
}
