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
    internal class OsuModApproachCircle : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Approach Circle";
        public override string Acronym => "AC";
        public override string Description => "Never trust the approach circles...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon { get; } = FontAwesome.Regular.Circle;

        [SettingSource("Easing", "Change the easing type of the approach circles.", 0)]
        public Bindable<Easing> BindableEasing { get; } = new Bindable<Easing>();

        [SettingSource("Scale the size", "Change the factor of the approach circle scale.", 1)]
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

                    var obj = drawableOsuHitObj.HitObject;

                    hitCircle.BeginAbsoluteSequence(obj.StartTime - obj.TimePreempt, true);
                    hitCircle.ApproachCircle.ScaleTo(Scale.Value);

                    hitCircle.ApproachCircle.FadeIn(Math.Min(obj.TimeFadeIn, obj.TimePreempt));
                    hitCircle.ApproachCircle.ScaleTo(1f, obj.TimePreempt, BindableEasing.Value);

                    hitCircle.ApproachCircle.Expire(true);
                };
            });
        }
    }
}
