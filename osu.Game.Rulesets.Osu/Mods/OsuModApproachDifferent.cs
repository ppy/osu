// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModApproachDifferent : Mod, IApplicableToDrawableHitObject, IRequiresApproachCircles
    {
        public override string Name => "Approach Different";
        public override string Acronym => "AD";
        public override string Description => "Never trust the approach circles...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon { get; } = FontAwesome.Regular.Circle;

        public override Type[] IncompatibleMods => new[] { typeof(IHidesApproachCircles) };

        [SettingSource("Initial size", "Change the initial size of the approach circle, relative to hit circles.", 0)]
        public BindableFloat Scale { get; } = new BindableFloat(4)
        {
            Precision = 0.1f,
            MinValue = 1.5f,
            MaxValue = 10,
        };

        [SettingSource("Style", "Change the animation style of the approach circles.", 1)]
        public Bindable<AnimationStyle> Style { get; } = new Bindable<AnimationStyle>(AnimationStyle.Gravity);

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.ApplyCustomUpdateState += (drawableObject, state) =>
            {
                if (!(drawableObject is DrawableHitCircle drawableHitCircle)) return;

                var hitCircle = drawableHitCircle.HitObject;

                drawableHitCircle.ApproachCircle.ClearTransforms(targetMember: nameof(Scale));

                using (drawableHitCircle.BeginAbsoluteSequence(hitCircle.StartTime - hitCircle.TimePreempt))
                    drawableHitCircle.ApproachCircle.ScaleTo(Scale.Value).ScaleTo(1f, hitCircle.TimePreempt, getEasing(Style.Value));
            };
        }

        private Easing getEasing(AnimationStyle style)
        {
            switch (style)
            {
                case AnimationStyle.Linear:
                    return Easing.None;

                case AnimationStyle.Gravity:
                    return Easing.InBack;

                case AnimationStyle.InOut1:
                    return Easing.InOutCubic;

                case AnimationStyle.InOut2:
                    return Easing.InOutQuint;

                case AnimationStyle.Accelerate1:
                    return Easing.In;

                case AnimationStyle.Accelerate2:
                    return Easing.InCubic;

                case AnimationStyle.Accelerate3:
                    return Easing.InQuint;

                case AnimationStyle.Decelerate1:
                    return Easing.Out;

                case AnimationStyle.Decelerate2:
                    return Easing.OutCubic;

                case AnimationStyle.Decelerate3:
                    return Easing.OutQuint;

                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, @"Unsupported animation style");
            }
        }

        public enum AnimationStyle
        {
            Linear,
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
