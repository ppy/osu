// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public partial class DrawablePerfectBonusNote : DrawableManiaHitObject<PerfectBonusNote>
    {
        public override bool DisplayResult => false;

        public DrawablePerfectBonusNote()
            : this(null!)
        {
        }

        public DrawablePerfectBonusNote(PerfectBonusNote hitObject)
            : base(hitObject)
        {
        }

        /// <summary>
        /// Apply a judgement result.
        /// </summary>
        /// <param name="hit">Whether this tick was reached.</param>
        internal void TriggerResult(bool hit) => ApplyResult(r => r.Type = hit ? r.Judgement.MaxResult : r.Judgement.MinResult);
    }
}
