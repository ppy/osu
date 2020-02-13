// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// Legacy osu!mania Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : HitObject, IHasEndTime, IHasXPosition
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        public float X { get; set; }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
