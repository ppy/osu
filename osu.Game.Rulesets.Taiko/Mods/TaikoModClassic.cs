// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Graphics;
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
        private LegacyMods legacyMods = LegacyMods.None;

        private DrawableTaikoRuleset? drawableTaikoRuleset;

        public void enableLegacyMods(LegacyMods legacyMods)
        {
            this.legacyMods = this.legacyMods | legacyMods;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspect.Value = false;

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

            drawableTaikoRuleset.TimeRange.Value = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;
        }

        private double aspectRatioToTimeRange(double aspectRatio)
        {
            return aspectRatio / (16.0f / 9.0f) * DrawableTaikoRuleset.default_time_range;
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

                        TaikoPlayfield playfield = (TaikoPlayfield)drawableTaikoRuleset.Playfield;
                        double maxTimeRange = aspectRatioToTimeRange(playfield.ClassicMaxAspectRatio.Value);
                        if (drawableTaikoRuleset.TimeRange.Value > maxTimeRange)
                        {
                            double preempt = drawableTaikoRuleset.TimeRange.Value / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;
                            double fadeInEnd = o.HitObject.StartTime - preempt * maxTimeRange / drawableTaikoRuleset.TimeRange.Value;
                            double fadeInStart = fadeInEnd - 500 / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;

                            // o.FadeOut(0);

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
