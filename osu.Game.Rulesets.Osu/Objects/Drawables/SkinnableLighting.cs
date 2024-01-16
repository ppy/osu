// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    internal partial class SkinnableLighting : SkinnableSprite
    {
        private DrawableHitObject targetObject;
        private Judgement targetJudgement;

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
        /// <param name="targetObject">The <see cref="DrawableHitObject"/> that's been judged.</param>
        /// <param name="targetJudgement">The <see cref="Judgement"/> that <paramref name="targetObject"/> was judged with.</param>
        public void SetColourFrom(DrawableHitObject targetObject, Judgement targetJudgement)
        {
            this.targetObject = targetObject;
            this.targetJudgement = targetJudgement;

            updateColour();
        }

        private void updateColour()
        {
            if (targetObject == null || targetJudgement == null)
                Colour = Color4.White;
            else
                Colour = targetJudgement.IsHit ? targetObject.AccentColour.Value : Color4.Transparent;
        }
    }
}
