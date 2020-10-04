// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Represents a component that displays a skinned <see cref="ICatchComboCounter"/> and handles combo judgement results for updating it accordingly.
    /// </summary>
    public class CatchComboDisplay : SkinnableDrawable
    {
        private int currentCombo;

        [CanBeNull]
        public ICatchComboCounter ComboCounter => Drawable as ICatchComboCounter;

        public CatchComboDisplay()
            : base(new CatchSkinComponent(CatchSkinComponents.CatchComboCounter), _ => Empty())
        {
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);
            ComboCounter?.UpdateCombo(currentCombo);
        }

        public void OnNewResult(DrawableCatchHitObject judgedObject, JudgementResult result)
        {
            if (!result.Type.AffectsCombo() || !result.HasResult)
                return;

            if (!result.IsHit)
            {
                updateCombo(0, null);
                return;
            }

            updateCombo(result.ComboAtJudgement + 1, judgedObject.AccentColour.Value);
        }

        public void OnRevertResult(DrawableCatchHitObject judgedObject, JudgementResult result)
        {
            if (!result.Type.AffectsCombo() || !result.HasResult)
                return;

            updateCombo(result.ComboAtJudgement, judgedObject.AccentColour.Value);
        }

        private void updateCombo(int newCombo, Color4? hitObjectColour)
        {
            currentCombo = newCombo;
            ComboCounter?.UpdateCombo(newCombo, hitObjectColour);
        }
    }
}
