// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public partial class DrawableNotePerfectBonus : DrawableManiaHitObject<NotePerfectBonus>
    {
        public override bool DisplayResult => false;

        public DrawableNotePerfectBonus()
            : this(null!)
        {
        }

        public DrawableNotePerfectBonus(NotePerfectBonus hitObject)
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
