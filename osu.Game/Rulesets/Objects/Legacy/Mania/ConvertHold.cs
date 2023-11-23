// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    internal sealed class ConvertHold : ConvertHitObject, IHasXPosition, IHasDuration
    {
        public float X { get; set; }

        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertHold convertHold)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertHold)}");

            X = convertHold.X;
            Duration = convertHold.Duration;
        }

        protected override HitObject CreateInstance() => new ConvertHold();
    }
}
