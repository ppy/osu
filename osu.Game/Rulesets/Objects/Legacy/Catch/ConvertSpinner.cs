// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : ConvertHitObject, IHasDuration, IHasXPosition, IHasCombo
    {
        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        public float X => 256; // Required for CatchBeatmapConverter

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSpinner convertSpinner)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSpinner)}");

            Duration = convertSpinner.Duration;
            NewCombo = convertSpinner.NewCombo;
            ComboOffset = convertSpinner.ComboOffset;
        }

        protected override HitObject CreateInstance() => new ConvertSpinner();
    }
}
