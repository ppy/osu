// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// Legacy osu! Hit-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertHit : ConvertHitObject, IHasPosition, IHasCombo
    {
        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertHit convertHit)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertHit)}");

            Position = convertHit.Position;
            NewCombo = convertHit.NewCombo;
            ComboOffset = convertHit.ComboOffset;
        }

        protected override HitObject CreateInstance() => new ConvertHit();
    }
}
