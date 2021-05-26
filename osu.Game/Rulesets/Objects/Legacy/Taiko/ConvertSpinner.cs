// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Taiko
{
    /// <summary>
    /// Legacy osu!taiko Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : ConvertHitObject, IHasDuration
    {
        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;
    }
}
