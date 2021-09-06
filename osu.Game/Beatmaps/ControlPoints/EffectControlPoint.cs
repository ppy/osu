// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class EffectControlPoint : ControlPoint
    {
        public static readonly EffectControlPoint DEFAULT = new EffectControlPoint
        {
            KiaiModeBindable = { Disabled = true },
            OmitFirstBarLineBindable = { Disabled = true },
            ScrollSpeedBindable = { Disabled = true }
        };

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public readonly BindableBool OmitFirstBarLineBindable = new BindableBool();

        /// <summary>
        /// The relative scroll speed at this control point.
        /// </summary>
        public readonly BindableDouble ScrollSpeedBindable = new BindableDouble(1)
        {
            Precision = 0.01,
            Default = 1,
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
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public bool OmitFirstBarLine
        {
            get => OmitFirstBarLineBindable.Value;
            set => OmitFirstBarLineBindable.Value = value;
        }

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

        public override bool IsRedundant(ControlPoint existing)
            => !OmitFirstBarLine
               && existing is EffectControlPoint existingEffect
               && KiaiMode == existingEffect.KiaiMode
               && OmitFirstBarLine == existingEffect.OmitFirstBarLine
               && ScrollSpeed == existingEffect.ScrollSpeed;

        public override void CopyFrom(ControlPoint other)
        {
            KiaiMode = ((EffectControlPoint)other).KiaiMode;
            OmitFirstBarLine = ((EffectControlPoint)other).OmitFirstBarLine;
            ScrollSpeed = ((EffectControlPoint)other).ScrollSpeed;

            base.CopyFrom(other);
        }
    }
}
