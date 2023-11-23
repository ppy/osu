// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSpinner spinner)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSpinner)}");

            Duration = spinner.Duration;
        }

        protected override HitObject CreateInstance() => new ConvertSpinner();
    }
}
