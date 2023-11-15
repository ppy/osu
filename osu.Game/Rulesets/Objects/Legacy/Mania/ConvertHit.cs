// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// Legacy osu!mania Hit-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertHit : ConvertHitObject, IHasXPosition
    {
        public float X { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertHit convertHit)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertHit)}");

            X = convertHit.X;
        }

        protected override HitObject CreateInstance() => new ConvertHit();
    }
}
