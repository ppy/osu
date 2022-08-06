// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Beatmaps.Legacy;
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
        /// <see cref="LegacyMods"/> enabled.
        /// </summary>
        private static readonly double classic_max_time_range = DrawableTaikoRuleset.AspectRatioToTimeRange(classic_max_aspect_ratio);

        /// <summary>
        /// The classic hidden aspect ratio. Note that time rate is also stretched to this from the default aspect ratio.
        /// </summary>
        private const float classic_hidden_aspect_ratio = 4.0f / 3.0f;

        private LegacyMods enabledMods = LegacyMods.None;

        private DrawableTaikoRuleset? drawableTaikoRuleset;

        private readonly Bindable<float> hiddenFadeOutDuration = new Bindable<float>(0.4f);

        private readonly Bindable<float> hiddenInitialAlpha = new Bindable<float>(0.65f);

        /// <summary>
        /// Mark a legacy mod as enabled. This class will then apply those mod's appropriate classic adjustments to the
        /// ruleset. These adjustments are aimed to match stable's behaviour.
        /// </summary>
        public void EnableLegacyMods(LegacyMods legacyMods)
        {
            enabledMods = enabledMods | legacyMods;
        }

        /// <summary>
        /// Binds hidden parameter, which will be udpated by this class.
        /// </summary>
        public void BindHiddenParameters(Bindable<float> hiddenFadeOutDuration, Bindable<float> hiddenInitialAlpha)
        {
            this.hiddenFadeOutDuration.BindTo(hiddenFadeOutDuration);
            this.hiddenInitialAlpha.BindTo(hiddenInitialAlpha);
            this.hiddenFadeOutDuration.Value = 0.4f;
            this.hiddenInitialAlpha.Value = 0.65f;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.None;

            var playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            playfield.ClassicHitTargetPosition.Value = true;
        }

        public void Update(Playfield playfield)
        {
            Debug.Assert(drawableTaikoRuleset != null);

            // Classic taiko scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            float ratio = drawableTaikoRuleset.DrawHeight / 480;

            double timeRange = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;

            if (enabledMods.HasFlagFast(LegacyMods.HardRock))
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
            else if (enabledMods.HasFlagFast(LegacyMods.Hidden))
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
                        if (enabledMods == LegacyMods.None)
                        {
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
                        }
                    };
                    break;
            }
        }
    }
}
