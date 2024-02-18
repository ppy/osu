// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToDrawableHitObject
    {
        private IReadOnlyList<Mod> mods = Array.Empty<Mod>();

        private const float hd_base_fade_out_duration = 0.375f;

        private const float hd_base_initial_alpha = 0.75f;

        private const float hdhr_base_fade_out_duration = 0.2f;

        private const float hdhr_base_initial_alpha = 0.2f;

        private const float hidden_base_aspect = 4f / 3f;

        private readonly BindableFloat hiddenFadeOutDuration = new BindableFloat(hd_base_fade_out_duration);

        private readonly BindableFloat hiddenInitialAlpha = new BindableFloat(hd_base_initial_alpha);

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            var drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            var adjustmentContainer = (TaikoPlayfieldAdjustmentContainer)drawableTaikoRuleset.PlayfieldAdjustmentContainer;

            // drawableRuleset.Mods should always be non-null here, but just in case.
            mods = drawableRuleset.Mods ?? mods;

            adjustmentContainer.MaximumAspect = 22f / 9f;
            adjustmentContainer.MinimumAspect = 5f / 4f;
            adjustmentContainer.TrimOnOverflow = true;

            TaikoModHidden? hidden = mods.OfType<TaikoModHidden>().FirstOrDefault();

            if (mods.OfType<TaikoModHardRock>().Any())
            {
                // For hardrock, the playfield time range is clamped to within classicMaxTimeRange and the equivalent
                // time range for a 16:10 aspect ratio.
                adjustmentContainer.TrimOnOverflow = false;

                // Apply stable aspect ratio limits for hardrock (visually taken)
                adjustmentContainer.MaximumAspect = 1.963f;

                // This is accurate to 4:3, but slightly off for 5:4
                adjustmentContainer.MinimumAspect = 1.666f;

                adjustmentContainer.MinimumRelativeHeight = 0.26f;
                adjustmentContainer.MaximumRelativeHeight = 0.26f;

                if (hidden != null)
                {
                    hiddenInitialAlpha.BindTo(hidden.InitialAlpha);
                    hiddenFadeOutDuration.BindTo(hidden.FadeOutDuration);
                    drawableRuleset.OnUpdate += d => adjustHidden(
                        d, hdhr_base_fade_out_duration, hdhr_base_initial_alpha, 16f / 9f, 0.8f);
                }
            }
            else if (hidden != null)
            {
                // Stable limits the aspect ratio to 4:3
                adjustmentContainer.MaximumAspect = hidden_base_aspect;

                // Enable aspect ratio adjustment for hidden (see adjustHidden)
                hiddenInitialAlpha.BindTo(hidden.InitialAlpha);
                hiddenFadeOutDuration.BindTo(hidden.FadeOutDuration);
                drawableRuleset.OnUpdate += d => adjustHidden(
                    d, hd_base_fade_out_duration, hd_base_initial_alpha, hidden_base_aspect);
            }
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableTaikoHitObject hit)
                hit.SnapJudgementLocation = true;
        }

        // Adjust hidden initial alpha and fade out duration for different aspect ratios
        private void adjustHidden(
            Drawable drawableRuleset,
            float baseFadeOutDuration,
            float baseInitialAlpha,
            float baseAspect,
            float adjustmentRatio = 1f)
        {
            var drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            var adjustmentContainer = (TaikoPlayfieldAdjustmentContainer)drawableTaikoRuleset.PlayfieldAdjustmentContainer;

            float aspect = Math.Clamp(
                adjustmentContainer.CurrentAspect,
                adjustmentContainer.MinimumAspect,
                adjustmentContainer.MaximumAspect);

            float fadeOutDurationAdjustment = aspect / baseAspect - 1;
            fadeOutDurationAdjustment *= adjustmentRatio;
            hiddenFadeOutDuration.Value = baseFadeOutDuration + fadeOutDurationAdjustment;

            float initialAlphaAdjustment = aspect / baseAspect - 1;
            initialAlphaAdjustment *= adjustmentRatio;
            hiddenInitialAlpha.Value = baseInitialAlpha + initialAlphaAdjustment;
        }
    }
}
