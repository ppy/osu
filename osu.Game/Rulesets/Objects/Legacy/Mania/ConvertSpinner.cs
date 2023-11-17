// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// Legacy osu!mania Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : ConvertHitObject, IHasDuration, IHasXPosition
    {
        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;

        public float X { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSpinner spinner)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSpinner)}");

            Duration = spinner.Duration;
            X = spinner.X;
        }

        protected override HitObject CreateInstance() => new ConvertSpinner();
    }
}
