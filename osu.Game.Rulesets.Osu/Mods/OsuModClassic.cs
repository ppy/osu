// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModClassic : ModClassic, IApplicableToHitObject, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModStrictTracking)).ToArray();

        [SettingSource("去除滑条头的准确率要求", "滑条分数与其命中的滑条刻成比例。")]
        public Bindable<bool> NoSliderHeadAccuracy { get; } = new BindableBool(true);

        [SettingSource("不要移动滑条头", "无论何时，都将滑条头固定在其起始位置。")]
        public Bindable<bool> NoSliderHeadMovement { get; } = new BindableBool(true);

        [SettingSource("应用v1 note锁", "将note锁应用与完整的打击窗口。")]
        public Bindable<bool> ClassicNoteLock { get; } = new BindableBool(true);

        [SettingSource("永远播放滑条尾音效", "总是播放滑条尾音效，无论该滑条是否已经滑完")]
        public Bindable<bool> AlwaysPlayTailSample { get; } = new BindableBool(true);

        [SettingSource("Fade out hit circles earlier", "Make hit circles fade out into a miss, rather than after it.")]
        public Bindable<bool> FadeHitCircleEarly { get; } = new Bindable<bool>(true);

        private bool usingHiddenFading;

        public void ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    slider.OnlyJudgeNestedObjects = !NoSliderHeadAccuracy.Value;

                    foreach (var head in slider.NestedHitObjects.OfType<SliderHeadCircle>())
                        head.JudgeAsNormalHitCircle = !NoSliderHeadAccuracy.Value;

                    break;
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            var osuRuleset = (DrawableOsuRuleset)drawableRuleset;

            if (ClassicNoteLock.Value)
                osuRuleset.Playfield.HitPolicy = new ObjectOrderedHitPolicy();

            usingHiddenFading = drawableRuleset.Mods.OfType<OsuModHidden>().SingleOrDefault()?.OnlyFadeApproachCircles.Value == false;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject obj)
        {
            switch (obj)
            {
                case DrawableSliderHead head:
                    head.TrackFollowCircle = !NoSliderHeadMovement.Value;
                    if (FadeHitCircleEarly.Value && !usingHiddenFading)
                        applyEarlyFading(head);
                    break;

                case DrawableSliderTail tail:
                    tail.SamplePlaysOnlyOnHit = !AlwaysPlayTailSample.Value;
                    break;

                case DrawableHitCircle circle:
                    if (FadeHitCircleEarly.Value && !usingHiddenFading)
                        applyEarlyFading(circle);
                    break;
            }
        }

        private void applyEarlyFading(DrawableHitCircle circle)
        {
            circle.ApplyCustomUpdateState += (o, _) =>
            {
                using (o.BeginAbsoluteSequence(o.StateUpdateTime))
                {
                    double okWindow = o.HitObject.HitWindows.WindowFor(HitResult.Ok);
                    double lateMissFadeTime = o.HitObject.HitWindows.WindowFor(HitResult.Meh) - okWindow;
                    o.Delay(okWindow).FadeOut(lateMissFadeTime);
                }
            };
        }
    }
}
