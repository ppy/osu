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

            // drawableRuleset.Mods should always be non-null here, but just in case.
            mods = drawableRuleset.Mods ?? mods;

            drawableTaikoRuleset.MaximumAspect.Value = 22f / 9f;
            drawableTaikoRuleset.MinimumAspect.Value = 5f / 4f;
            drawableTaikoRuleset.TrimOnOverflow.Value = true;

            TaikoModHidden? hidden = mods.OfType<TaikoModHidden>().FirstOrDefault();

            if (mods.OfType<TaikoModHardRock>().Any())
            {
                // For hardrock, the playfield time range is clamped to within classicMaxTimeRange and the equivalent
                // time range for a 16:10 aspect ratio.
                drawableTaikoRuleset.TrimOnOverflow.Value = false;

                // Apply stable aspect ratio limits for hardrock (visually taken)
                drawableTaikoRuleset.MaximumAspect.Value = 1.963f;

                // This is accurate to 4:3, but slightly off for 5:4
                drawableTaikoRuleset.MinimumAspect.Value = 1.666f;

                // Visually taken from different aspect ratios
                drawableTaikoRuleset.MinimumRelativeHeight.Value = 0.26f;
                drawableTaikoRuleset.MaximumRelativeHeight.Value = 0.26f;

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
                drawableTaikoRuleset.MaximumAspect.Value = hidden_base_aspect;

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

        // Compensate for aspect ratios narrower than 4:3 by scaling the fade out duration and initial alpha
        private void adjustHidden(
            Drawable drawableRuleset,
            float baseFadeOutDuration,
            float baseInitialAlpha,
            float baseAspect,
            float adjustmentRatio = 1f)
        {
            var drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            float aspect = Math.Clamp(
                drawableTaikoRuleset.CurrentAspect.Value,
                drawableTaikoRuleset.MinimumAspect.Value,
                drawableTaikoRuleset.MaximumAspect.Value);

            float fadeOutDurationAdjustment = aspect / baseAspect - 1;
            fadeOutDurationAdjustment *= adjustmentRatio;
            hiddenFadeOutDuration.Value = baseFadeOutDuration + fadeOutDurationAdjustment;

            float initialAlphaAdjustment = aspect / baseAspect - 1;
            initialAlphaAdjustment *= adjustmentRatio;
            hiddenInitialAlpha.Value = baseInitialAlpha + initialAlphaAdjustment;
        }
    }
}
