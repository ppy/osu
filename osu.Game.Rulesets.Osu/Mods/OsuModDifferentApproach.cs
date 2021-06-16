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
    internal class OsuModDifferentApproach : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Approach Different";
        public override string Acronym => "AD";
        public override string Description => "Never trust the approach circles...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon { get; } = FontAwesome.Regular.Circle;

        [SettingSource("Easing", "Change the animation curve of the approach circles.", 0)]
        public Bindable<ApproachCircleEasing> BindableEasing { get; } = new Bindable<ApproachCircleEasing>();

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

                    hitCircle.ApproachCircle.ScaleTo(1f, obj.TimePreempt, getEasing(BindableEasing.Value));

                    hitCircle.ApproachCircle.Expire(true);
                };
            });
        }

        private Easing getEasing(ApproachCircleEasing approachEasing)
        {
            switch (approachEasing)
            {
                default:
                    return Easing.None;

                case ApproachCircleEasing.Accelerate1:
                    return Easing.In;

                case ApproachCircleEasing.Accelerate2:
                    return Easing.InCubic;

                case ApproachCircleEasing.Accelerate3:
                    return Easing.InQuint;

                case ApproachCircleEasing.Gravity:
                    return Easing.InBack;

                case ApproachCircleEasing.Decelerate1:
                    return Easing.Out;

                case ApproachCircleEasing.Decelerate2:
                    return Easing.OutCubic;

                case ApproachCircleEasing.Decelerate3:
                    return Easing.OutQuint;

                case ApproachCircleEasing.InOut1:
                    return Easing.InOutCubic;

                case ApproachCircleEasing.InOut2:
                    return Easing.InOutQuint;
            }
        }

        public enum ApproachCircleEasing
        {
            Default,
            Accelerate1,
            Accelerate2,
            Accelerate3,
            Gravity,
            Decelerate1,
            Decelerate2,
            Decelerate3,
            InOut1,
            InOut2,
        }
    }
}
