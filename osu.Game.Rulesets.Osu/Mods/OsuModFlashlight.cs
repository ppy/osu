// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlashlight : ModFlashlight<OsuHitObject>, IApplicableToDrawableHitObject
    {
        public override double ScoreMultiplier => 1.12;

        private const double default_follow_delay = 120;

        [SettingSource("Follow delay", "Milliseconds until the flashlight reaches the cursor")]
        public BindableNumber<double> FollowDelay { get; } = new BindableDouble(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = default_follow_delay * 10,
            Precision = default_follow_delay,
        };

        [SettingSource("Change radius based on combo", "Decrease the flashlight radius as combo increases.")]
        public override BindableBool ChangeRadius { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Initial radius", "Initial radius of the flashlight area.")]
        public override BindableNumber<float> InitialRadius { get; } = new BindableNumber<float>
        {
            MinValue = 90f,
            MaxValue = 360f,
            Default = 180f,
            Value = 180f,
            Precision = 5f
        };

        private OsuFlashlight flashlight;

        public override Flashlight CreateFlashlight() => flashlight = new OsuFlashlight(ChangeRadius.Value, InitialRadius.Value, FollowDelay.Value);

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSlider s)
                s.Tracking.ValueChanged += flashlight.OnSliderTrackingChange;
        }

        private class OsuFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            public double FollowDelay { private get; set; }

            //public float InitialRadius { private get; set; }
            public bool ChangeRadius { private get; set; }

            public OsuFlashlight(bool isRadiusBasedOnCombo, float initialRadius, double followDelay)
                : base(isRadiusBasedOnCombo, initialRadius)
            {
                FollowDelay = followDelay;

                FlashlightSize = new Vector2(0, GetRadiusFor(0));
            }

            public void OnSliderTrackingChange(ValueChangedEvent<bool> e)
            {
                // If a slider is in a tracking state, a further dim should be applied to the (remaining) visible portion of the playfield over a brief duration.
                this.TransformTo(nameof(FlashlightDim), e.NewValue ? 0.8f : 0.0f, 50);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    Math.Min(Math.Abs(Clock.ElapsedFrameTime), FollowDelay), position, destination, 0, FollowDelay, Easing.Out);

                return base.OnMouseMove(e);
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, GetRadiusFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
