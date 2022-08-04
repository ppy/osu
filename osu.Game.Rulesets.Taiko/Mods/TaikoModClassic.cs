// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
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

        private LegacyMods enabledMods = LegacyMods.None;

        private DrawableTaikoRuleset? drawableTaikoRuleset;

        private double classicMaxTimeRange;

        public void enableLegacyMods(LegacyMods legacyMods)
        {
            this.enabledMods = this.enabledMods | legacyMods;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspect.Value = false;

            var playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            playfield.ClassicHitTargetPosition.Value = true;

            classicMaxTimeRange = aspectRatioToTimeRange(classic_max_aspect_ratio);
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
                timeRange = Math.Min(timeRange, classicMaxTimeRange);
            }
            drawableTaikoRuleset.TimeRange.Value = timeRange;
        }

        private double aspectRatioToTimeRange(double aspectRatio)
        {
            return aspectRatio / TaikoPlayfieldAdjustmentContainer.default_aspect * DrawableTaikoRuleset.default_time_range;
        }

        void IApplicableToDrawableHitObject.ApplyToDrawableHitObject(DrawableHitObject hitObject)
        {
            if (enabledMods == LegacyMods.None)
            {
                switch (hitObject)
                {
                    case DrawableDrumRoll:
                    case DrawableDrumRollTick:
                    case DrawableHit:
                        hitObject.ApplyCustomUpdateState += (o, state) =>
                        {
                            Debug.Assert(drawableTaikoRuleset != null);

                            TaikoPlayfield playfield = (TaikoPlayfield)drawableTaikoRuleset.Playfield;
                            if (drawableTaikoRuleset.TimeRange.Value > classicMaxTimeRange)
                            {
                                double preempt = drawableTaikoRuleset.TimeRange.Value / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;
                                double fadeInEnd = o.HitObject.StartTime - preempt * classicMaxTimeRange / drawableTaikoRuleset.TimeRange.Value;
                                double fadeInStart = fadeInEnd - 500 / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;

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
}
