// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects.Legacy.Taiko
{
    /// <summary>
    /// Legacy osu!taiko Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : HitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
