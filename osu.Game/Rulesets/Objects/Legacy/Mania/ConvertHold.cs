// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    internal sealed class ConvertHold : ConvertHitObject, IHasXPosition, IHasDuration
    {
        public float X { get; set; }

        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;
    }
}
