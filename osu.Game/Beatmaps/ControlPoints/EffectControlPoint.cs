// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class EffectControlPoint : ControlPoint
    {
        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public readonly BindableBool OmitFirstBarLineBindable = new BindableBool();

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

        public override bool EquivalentTo(ControlPoint other) =>
            other is EffectControlPoint otherTyped &&
            KiaiMode == otherTyped.KiaiMode && OmitFirstBarLine == otherTyped.OmitFirstBarLine;
    }
}
