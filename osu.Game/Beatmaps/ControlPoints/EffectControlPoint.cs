// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class EffectControlPoint : ControlPoint, IEquatable<EffectControlPoint>
    {
        public static readonly EffectControlPoint DEFAULT = new EffectControlPoint
        {
            KiaiModeBindable = { Disabled = true },
            ScrollSpeedBindable = { Disabled = true }
        };

        /// <summary>
        /// The relative scroll speed at this control point.
        /// </summary>
        public readonly BindableDouble ScrollSpeedBindable = new BindableDouble(1)
        {
            Precision = 0.01,
            MinValue = 0.01,
            MaxValue = 10
        };

        /// <summary>
        /// The relative scroll speed.
        /// </summary>
        public double ScrollSpeed
        {
            get => ScrollSpeedBindable.Value;
            set => ScrollSpeedBindable.Value = value;
        }

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.Purple;

        /// <summary>
        /// Whether this control point enables Kiai mode.
        /// </summary>
        public readonly BindableBool KiaiModeBindable = new BindableBool();

        /// <summary>
        /// Whether this control point enables Kiai mode.
        /// </summary>
        public bool KiaiMode
        {
            get => KiaiModeBindable.Value;
            set => KiaiModeBindable.Value = value;
        }

        public override bool IsRedundant(ControlPoint? existing)
            => existing is EffectControlPoint existingEffect
               && KiaiMode == existingEffect.KiaiMode
               && ScrollSpeed == existingEffect.ScrollSpeed;

        public override void CopyFrom(ControlPoint other)
        {
            KiaiMode = ((EffectControlPoint)other).KiaiMode;
            ScrollSpeed = ((EffectControlPoint)other).ScrollSpeed;

            base.CopyFrom(other);
        }

        public override bool Equals(ControlPoint? other)
            => other is EffectControlPoint otherEffectControlPoint
               && Equals(otherEffectControlPoint);

        public bool Equals(EffectControlPoint? other)
            => base.Equals(other)
               && ScrollSpeed == other.ScrollSpeed
               && KiaiMode == other.KiaiMode;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ScrollSpeed, KiaiMode);
    }
}
