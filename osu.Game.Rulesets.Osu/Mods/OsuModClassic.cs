// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModClassic : ModClassic, IApplicableToHitObject, IApplicableToDrawableHitObjects, IApplicableToDrawableRuleset<OsuHitObject>
    {
        [SettingSource("去除滑条头的准确率要求", "滑条分数与其命中的滑条刻成比例。")]
        public Bindable<bool> NoSliderHeadAccuracy { get; } = new BindableBool(true);

        [SettingSource("不要移动滑条头", "无论何时，都将滑条头固定在其起始位置。")]
        public Bindable<bool> NoSliderHeadMovement { get; } = new BindableBool(true);

        [SettingSource("应用v1 note锁", "将note锁应用与完整的打击窗口。")]
        public Bindable<bool> ClassicNoteLock { get; } = new BindableBool(true);

        [SettingSource("固定滑条球打击区域", "使滑条球始终跟踪其最终大小。")]
        public Bindable<bool> FixedFollowCircleHitArea { get; } = new BindableBool(true);

        [SettingSource("永远播放滑条尾音效", "总是播放滑条尾音效，无论该滑条是否已经画完")]
        public Bindable<bool> AlwaysPlayTailSample { get; } = new BindableBool(true);

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
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var obj in drawables)
            {
                switch (obj)
                {
                    case DrawableSlider slider:
                        slider.Ball.InputTracksVisualSize = !FixedFollowCircleHitArea.Value;
                        break;

                    case DrawableSliderHead head:
                        head.TrackFollowCircle = !NoSliderHeadMovement.Value;
                        break;

                    case DrawableSliderTail tail:
                        tail.SamplePlaysOnlyOnHit = !AlwaysPlayTailSample.Value;
                        break;
                }
            }
        }
    }
}
