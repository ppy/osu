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
    public partial class OsuModFlashlight : ModFlashlight<OsuHitObject>, IApplicableToDrawableHitObject
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModBlinds)).ToArray();

        private const double default_follow_delay = 120;

        [SettingSource("Follow delay", "Milliseconds until the flashlight reaches the cursor")]
        public BindableNumber<double> FollowDelay { get; } = new BindableDouble(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = default_follow_delay * 10,
            Precision = default_follow_delay,
        };

        public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
        {
            MinValue = 0.5f,
            MaxValue = 2f,
            Precision = 0.1f
        };

        public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

        public override float DefaultFlashlightSize => 200;

        private OsuFlashlight flashlight = null!;

        protected override Flashlight CreateFlashlight() => flashlight = new OsuFlashlight(this);

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSlider s)
                s.OnUpdate += _ => flashlight.OnSliderTrackingChange(s);
        }

        private partial class OsuFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            private readonly double followDelay;

            public OsuFlashlight(OsuModFlashlight modFlashlight)
                : base(modFlashlight)
            {
                followDelay = modFlashlight.FollowDelay.Value;

                FlashlightSize = new Vector2(0, GetSize());
                FlashlightSmoothness = 1.4f;
            }

            public void OnSliderTrackingChange(DrawableSlider e)
            {
                // If a slider is in a tracking state, a further dim should be applied to the (remaining) visible portion of the playfield.
                FlashlightDim = Time.Current >= e.HitObject.StartTime && e.Tracking.Value ? 0.8f : 0.0f;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    Math.Min(Math.Abs(Clock.ElapsedFrameTime), followDelay), position, destination, 0, followDelay, Easing.Out);

                return base.OnMouseMove(e);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, size), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
