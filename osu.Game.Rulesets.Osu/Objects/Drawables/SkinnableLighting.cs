// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    internal partial class SkinnableLighting : SkinnableSprite
    {
        private DrawableOsuJudgement? targetJudgement;
        private JudgementResult? targetResult;

        public SkinnableLighting()
            : base("lighting")
        {
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            updateColour();
        }

        /// <summary>
        /// Updates the lighting colour from a given hitobject and result.
        /// </summary>
        /// <param name="targetJudgement">The <see cref="DrawableHitObject"/> that's been judged.</param>
        /// <param name="targetResult">The <see cref="JudgementResult"/> that <paramref name="targetJudgement"/> was judged with.</param>
        public void SetColourFrom(DrawableOsuJudgement targetJudgement, JudgementResult? targetResult)
        {
            this.targetJudgement = targetJudgement;
            this.targetResult = targetResult;

            updateColour();
        }

        private void updateColour()
        {
            if (targetJudgement == null || targetResult == null)
                Colour = Color4.White;
            else
                Colour = targetResult.IsHit && !targetResult.Type.IsTick() ? targetJudgement.AccentColour : Color4.Transparent;
        }
    }
}
