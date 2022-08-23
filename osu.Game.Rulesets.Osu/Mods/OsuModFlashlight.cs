// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
    public class OsuModFlashlight : ModFlashlight<OsuHitObject>, IApplicableToDrawableHitObject, ICanBeToggledDuringReplay
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModBlinds)).ToArray();

        private const double default_follow_delay = 120;

        private readonly Bindable<bool> isDisabled = new Bindable<bool>();

        public Bindable<bool> ReplayLoaded { get; } = new Bindable<bool>();

        public bool IsDisable => isDisabled.Value;

        [SettingSource("Follow delay", "Milliseconds until the flashlight reaches the cursor")]
        public BindableNumber<double> FollowDelay { get; } = new BindableDouble(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = default_follow_delay * 10,
            Precision = default_follow_delay,
        };

        [SettingSource("Flashlight size", "Multiplier applied to the default flashlight size.")]
        public override BindableFloat SizeMultiplier { get; } = new BindableFloat
        {
            MinValue = 0.5f,
            MaxValue = 2f,
            Default = 1f,
            Value = 1f,
            Precision = 0.1f
        };

        [SettingSource("Change size based on combo", "Decrease the flashlight size as combo increases.")]
        public override BindableBool ComboBasedSize { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public override float DefaultFlashlightSize => 180;

        private OsuFlashlight flashlight = null!;

        protected override Flashlight CreateFlashlight() => flashlight = new OsuFlashlight(this, isDisabled);

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSlider s)
                s.Tracking.ValueChanged += flashlight.OnSliderTrackingChange;
        }

        public void OnToggle()
        {
            if (!ReplayLoaded.Value) return;

            if (isDisabled.Value)
            {
                flashlight.Alpha = 1f;
                isDisabled.Value = false;
            }
            else
            {
                flashlight.Alpha = 0.3f;
                isDisabled.Value = true;
            }
        }

        private class OsuFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            private readonly double followDelay;

            private readonly Bindable<bool> isDisable;

            public OsuFlashlight(OsuModFlashlight modFlashlight, Bindable<bool> isDisableBind)
                : base(modFlashlight)
            {
                followDelay = modFlashlight.FollowDelay.Value;

                FlashlightSize = new Vector2(0, GetSizeFor(0));

                isDisable = isDisableBind;

                isDisable.BindValueChanged(disable =>
                {
                    if (disable.NewValue) this.TransformTo(nameof(FlashlightDim), 0.0f, 50);
                }, true);
            }

            public void OnSliderTrackingChange(ValueChangedEvent<bool> e)
            {
                if (isDisable.Value)
                    return;
                // If a slider is in a tracking state, a further dim should be applied to the (remaining) visible portion of the playfield over a brief duration.
                this.TransformTo(nameof(FlashlightDim), e.NewValue ? 0.8f : 0.0f, 50);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    Math.Min(Math.Abs(Clock.ElapsedFrameTime), followDelay), position, destination, 0, followDelay, Easing.Out);

                return base.OnMouseMove(e);
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, GetSizeFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
