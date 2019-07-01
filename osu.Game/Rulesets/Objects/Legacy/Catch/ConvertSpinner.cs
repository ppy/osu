// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : HitObject, IHasEndTime, IHasXPosition, IHasCombo
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        public float X => 256; // Required for CatchBeatmapConverter

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }
    }
}
