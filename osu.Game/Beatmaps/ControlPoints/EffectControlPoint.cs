// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps.ControlPoints
{
    public class EffectControlPoint : ControlPoint
    {
        /// <summary>
        /// Whether this control point enables Kiai mode.
        /// </summary>
        public bool KiaiMode;

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public bool OmitFirstBarLine;

        public override bool EquivalentTo(ControlPoint other)
            => base.EquivalentTo(other)
               && other is EffectControlPoint effect
               && KiaiMode.Equals(effect.KiaiMode)
               && OmitFirstBarLine.Equals(effect.OmitFirstBarLine);
    }
}
