// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToDrawableHitObject, IUpdatableByPlayfield
    {
        /// <summary>
        /// The maximum aspect ratio with classic mod enabled.
        /// </summary>
        private const float classic_max_aspect_ratio = 2.0f / 1.0f;

        /// <summary>
        /// The maximum time range with classic mod enabled. This will be enforced in different ways depending on the
        /// <see cref="Mod"/>s enabled.
        /// </summary>
        private static readonly double classic_max_time_range = DrawableTaikoRuleset.AspectRatioToTimeRange(classic_max_aspect_ratio);

        /// <summary>
        /// The classic hidden aspect ratio. Note that time range is also stretched to this from the default aspect ratio.
        /// </summary>
        private const float classic_hidden_aspect_ratio = 4.0f / 3.0f;

        private DrawableTaikoRuleset? drawableTaikoRuleset;

        private readonly BindableFloat hiddenFadeOutDuration = new BindableFloat(0.4f);

        private readonly BindableFloat hiddenInitialAlpha = new BindableFloat(0.65f);

        private IReadOnlyList<Mod>? mods;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.None;

            var playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            playfield.ClassicHitTargetPosition.Value = true;

            mods = drawableRuleset.Mods;

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case TaikoModHidden taikoModHidden:
                        hiddenFadeOutDuration.BindTo(taikoModHidden.FadeOutDuration);
                        hiddenInitialAlpha.BindTo(taikoModHidden.InitialAlpha);
                        break;
                }
            }
        }

        public void Update(Playfield playfield)
        {
            Debug.Assert(drawableTaikoRuleset != null && mods != null);

            // Classic taiko scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            float ratio = drawableTaikoRuleset.DrawHeight / 480;

            double timeRange = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;

            if (mods.OfType<TaikoModHardRock>().Any())
            {
                // For hardrock, the playfield time range is clamped to within classicMaxTimeRange and the equivalent
                // time range for a 16:10 aspect ratio.
                drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.None;
                timeRange = Math.Clamp(timeRange, DrawableTaikoRuleset.AspectRatioToTimeRange(1.6f), classic_max_time_range);

                // Scale hidden parameter to match the adjusted time range. This only affects hdhr.
                float hiddenRatio = 1.0f - Math.Min((float)timeRange / (float)DrawableTaikoRuleset.DEFAULT_TIME_RANGE, 1.0f);
                float fadeOutDuration = Math.Max(0.24f - hiddenRatio, 0.1f);
                hiddenInitialAlpha.Value = 0.3f * fadeOutDuration / 0.2f;
                hiddenFadeOutDuration.Value = fadeOutDuration;
            }
            else if (mods.OfType<TaikoModHidden>().Any())
            {
                // Hidden aspect adjustment is overriden by hardrock in the case of hdhr, hence these are applied only
                // if hardrock is not enabled.
                drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.Trim;
                drawableTaikoRuleset.TargetAspectRatio.Value = classic_hidden_aspect_ratio;
            }

            drawableTaikoRuleset.TimeRange.Value = timeRange;
        }

        void IApplicableToDrawableHitObject.ApplyToDrawableHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableDrumRoll:
                case DrawableDrumRollTick:
                case DrawableHit:
                    hitObject.ApplyCustomUpdateState += (o, state) =>
                    {
                        Debug.Assert(drawableTaikoRuleset != null);

                        // For classic nomod, the effective aspect ratio will be limited to 2:1 by fading the notes in.
                        // We do not need to explicitly detect mods here, as both hidden and hardrock in classic limits
                        // the time range to under the 2:1 limit.
                        if (drawableTaikoRuleset.TimeRange.Value > classic_max_time_range)
                        {
                            o.Alpha = 0;

                            double preempt = drawableTaikoRuleset.TimeRange.Value / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;
                            double fadeInEnd = o.HitObject.StartTime - preempt * classic_max_time_range / drawableTaikoRuleset.TimeRange.Value;
                            double fadeInStart = fadeInEnd - 2000 / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;

                            using (o.BeginAbsoluteSequence(fadeInStart))
                            {
                                o.FadeIn(fadeInEnd - fadeInStart);
                            }
                        }
                    };
                    break;
            }
        }
    }
}
