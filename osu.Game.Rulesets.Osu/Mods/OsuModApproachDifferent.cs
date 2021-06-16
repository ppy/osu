// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModApproachDifferent : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Approach Different";
        public override string Acronym => "AD";
        public override string Description => "Never trust the approach circles...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon { get; } = FontAwesome.Regular.Circle;

        [SettingSource("Style", "Change the animation style of the approach circles.", 0)]
        public Bindable<AnimationStyle> Style { get; } = new Bindable<AnimationStyle>();

        [SettingSource("Initial size", "Change the initial size of the approach circle, relative to hit circles.", 1)]
        public BindableFloat Scale { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 2,
            MaxValue = 10,
            Default = 4,
            Value = 4
        };

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            drawables.ForEach(drawable =>
            {
                drawable.ApplyCustomUpdateState += (drawableHitObj, state) =>
                {
                    if (!(drawableHitObj is DrawableHitCircle hitCircle)) return;

                    var obj = hitCircle.HitObject;

                    hitCircle.BeginAbsoluteSequence(obj.StartTime - obj.TimePreempt, true);
                    hitCircle.ApproachCircle.ScaleTo(Scale.Value);

                    hitCircle.ApproachCircle.FadeIn(Math.Min(obj.TimeFadeIn, obj.TimePreempt));

                    hitCircle.ApproachCircle.ScaleTo(1f, obj.TimePreempt, getEasing(Style.Value));

                    hitCircle.ApproachCircle.Expire(true);
                };
            });
        }

        private Easing getEasing(AnimationStyle approachEasing)
        {
            switch (approachEasing)
            {
                default:
                    return Easing.None;

                case AnimationStyle.Accelerate1:
                    return Easing.In;

                case AnimationStyle.Accelerate2:
                    return Easing.InCubic;

                case AnimationStyle.Accelerate3:
                    return Easing.InQuint;

                case AnimationStyle.Gravity:
                    return Easing.InBack;

                case AnimationStyle.Decelerate1:
                    return Easing.Out;

                case AnimationStyle.Decelerate2:
                    return Easing.OutCubic;

                case AnimationStyle.Decelerate3:
                    return Easing.OutQuint;

                case AnimationStyle.InOut1:
                    return Easing.InOutCubic;

                case AnimationStyle.InOut2:
                    return Easing.InOutQuint;
            }
        }

        public enum AnimationStyle
        {
            Gravity,
            InOut1,
            InOut2,
            Accelerate1,
            Accelerate2,
            Accelerate3,
            Decelerate1,
            Decelerate2,
            Decelerate3,
        }
    }
}
