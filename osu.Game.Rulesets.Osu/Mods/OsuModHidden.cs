// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHidden : ModHidden, IHidesApproachCircles, IApplicableToDrawableRuleset<OsuHitObject>
    {
        [SettingSource("Only fade approach circles", "The main object body will not fade when enabled.")]
        public Bindable<bool> OnlyFadeApproachCircles { get; } = new BindableBool();

        [SettingSource("Enable at combo", "The combo at which the hidden effect will take full effect. 0 for always.")]
        public BindableNumber<int> EnableAtCombo { get; } = new BindableNumber<int>()
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 1,
        };

        public override LocalisableString Description => @"Play with no approach circles and fading circles/sliders.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles), typeof(OsuModSpinIn), typeof(OsuModDepth), typeof(OsuModFreezeFrame) };

        public const double FADE_IN_DURATION_MULTIPLIER = 0.4;
        public const double FADE_OUT_DURATION_MULTIPLIER = 0.3;
        private Playfield playfield = null!;

        protected override int EnableAtComboValue => EnableAtCombo.Value;

        protected override bool IsFirstAdjustableObject(HitObject hitObject) => !(hitObject is Spinner || hitObject is SpinnerTick);

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * FADE_IN_DURATION_MULTIPLIER;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyHiddenState(hitObject, true);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            applyHiddenState(hitObject, false);
        }

        protected override uint GetHiddenComboInfluence(JudgementResult judgementResult) => judgementResult.HitObject switch
        {
            HitCircle and not SliderEndCircle => 1,
            _ => 0,
        };

        private void applyHiddenState(DrawableHitObject drawableObject, bool increaseVisibility)
        {
            if (!(drawableObject is DrawableOsuHitObject drawableOsuObject))
                return;

            OsuHitObject hitObject = drawableOsuObject.HitObject;

            (double fadeStartTime, double fadeDuration) = getFadeOutParameters(drawableOsuObject);

            // process approach circle hiding first (to allow for early return below).
            if (drawableObject is DrawableHitCircle circle2 && !circle2.Result.HasResult)
            {
                float alpha = increaseVisibility ? 1 : GetAndUpdateDrawableHitObjectComboAlpha(circle2);

                using (circle2.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt))
                    circle2.ApproachCircle.FadeTo(alpha * 0.9f, Math.Min(hitObject.TimeFadeIn * 2, hitObject.TimePreempt));
            }
            else if (!increaseVisibility && drawableObject is DrawableSpinner spinner)
            {
                spinner.Body.OnSkinChanged += () => hideSpinnerApproachCircle(spinner);
                hideSpinnerApproachCircle(spinner);
            }

            if (OnlyFadeApproachCircles.Value)
                return;

            switch (drawableObject)
            {
                case DrawableSliderTail:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        drawableObject.FadeOut(fadeDuration);

                    break;

                case DrawableSliderRepeat sliderRepeat:
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        // only apply to circle piece – reverse arrow is not affected by hidden.
                        sliderRepeat.CirclePiece.FadeOut(fadeDuration);

                    using (drawableObject.BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                        sliderRepeat.FadeOut();

                    break;

                case DrawableHitCircle circle:
                    // Only fade the circle piece, so that approach circles can be faded independently above
                    Drawable fadeTarget = circle.CirclePiece;
                    using (drawableObject.BeginAbsoluteSequence(fadeStartTime))
                        fadeTarget.FadeOut(fadeDuration);
                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(fadeStartTime))
                        slider.Body.FadeOut(fadeDuration, Easing.Out);

                    break;

                case DrawableSliderTick sliderTick:
                    using (sliderTick.BeginAbsoluteSequence(fadeStartTime))
                        sliderTick.FadeOut(fadeDuration);

                    break;

                case DrawableSpinner spinner:
                    // hide elements we don't care about.
                    // todo: hide background

                    using (spinner.BeginAbsoluteSequence(fadeStartTime))
                        spinner.FadeOut(fadeDuration);

                    break;
            }
        }

        private (double fadeStartTime, double fadeDuration) getFadeOutParameters(DrawableOsuHitObject drawableObject)
        {
            switch (drawableObject)
            {
                case DrawableSliderTail tail:
                    // Use the same fade sequence as the slider head.
                    Debug.Assert(tail.Slider != null);
                    return getParameters(tail.Slider.HeadCircle);

                case DrawableSliderRepeat repeat:
                    // Use the same fade sequence as the slider head.
                    Debug.Assert(repeat.Slider != null);
                    return getParameters(repeat.Slider.HeadCircle);

                default:
                    return getParameters(drawableObject.HitObject);
            }

            static (double fadeStartTime, double fadeDuration) getParameters(OsuHitObject hitObject)
            {
                double fadeOutStartTime = hitObject.StartTime - hitObject.TimePreempt + hitObject.TimeFadeIn;
                double fadeOutDuration = hitObject.TimePreempt * FADE_OUT_DURATION_MULTIPLIER;

                // new duration from completed fade in to end (before fading out)
                double longFadeDuration = hitObject.GetEndTime() - fadeOutStartTime;

                switch (hitObject)
                {
                    case Slider:
                        return (fadeOutStartTime, longFadeDuration);

                    case SliderTick:
                        double tickFadeOutDuration = Math.Min(hitObject.TimePreempt - DrawableSliderTick.ANIM_DURATION, 1000);
                        return (hitObject.StartTime - tickFadeOutDuration, tickFadeOutDuration);

                    case Spinner:
                        return (fadeOutStartTime + longFadeDuration, fadeOutDuration);

                    default:
                        return (fadeOutStartTime, fadeOutDuration);
                }
            }
        }

        private static void hideSpinnerApproachCircle(DrawableSpinner spinner)
        {
            var approachCircle = (spinner.Body.Drawable as IHasApproachCircle)?.ApproachCircle;
            if (approachCircle == null)
                return;

            using (spinner.BeginAbsoluteSequence(spinner.HitObject.StartTime - spinner.HitObject.TimePreempt))
                approachCircle.Hide();
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            playfield = drawableRuleset.Playfield;
        }

        public override Playfield PlayfieldAccessor => playfield;
    }
}
