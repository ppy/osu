// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModWiggle : ModWithVisibilityAdjustment
    {
        public override string Name => "Wiggle";
        public override string Acronym => "WG";
        public override IconUsage? Icon => FontAwesome.Solid.Certificate;
        public override ModType Type => ModType.Fun;
        public override string Description => "They just won't stay still...";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModTransform) };

        [SettingSource("Strength")]
        public BindableDouble WiggleStrength { get; } = new BindableDouble(10)
        {
            MinValue = 1f,
            MaxValue = 15f,
            Precision = .5f
        };

        [SettingSource("Duration", "Milliseconds per wiggle")]
        public BindableInt WiggleDuration { get; } = new BindableInt(90)
        {
            MinValue = 40,
            MaxValue = 300,
            Precision = 5
        };

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => drawableOnApplyCustomUpdateState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => drawableOnApplyCustomUpdateState(hitObject, state);

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var osuObject = (OsuHitObject)drawable.HitObject;
            Vector2 origin = drawable.Position;

            // Wiggle the repeat points and the tail with the slider instead of independently.
            // Also fixes an issue with repeat points being positioned incorrectly.
            if (osuObject is SliderRepeat || osuObject is SliderTailCircle)
                return;

            Random objRand = new Random((int)osuObject.StartTime);

            // Wiggle all objects during TimePreempt
            int amountWiggles = (int)osuObject.TimePreempt / WiggleDuration.Value;

            void wiggle()
            {
                float nextAngle = (float)(objRand.NextDouble() * 2 * Math.PI);
                float nextDist = (float)(objRand.NextDouble() * WiggleStrength.Value);
                drawable.MoveTo(new Vector2((float)(nextDist * Math.Cos(nextAngle) + origin.X), (float)(nextDist * Math.Sin(nextAngle) + origin.Y)), WiggleDuration.Value);
            }

            for (int i = 0; i < amountWiggles; i++)
            {
                using (drawable.BeginAbsoluteSequence(osuObject.StartTime - osuObject.TimePreempt + i * WiggleDuration.Value))
                    wiggle();
            }

            // Keep wiggling sliders and spinners for their duration
            if (!(osuObject is IHasDuration endTime))
                return;

            amountWiggles = (int)(endTime.Duration / WiggleDuration.Value);

            for (int i = 0; i < amountWiggles; i++)
            {
                using (drawable.BeginAbsoluteSequence(osuObject.StartTime + i * WiggleDuration.Value))
                    wiggle();
            }
        }
    }
}
