// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlashlight : ModFlashlight<OsuHitObject>, IApplicableToDrawableHitObjects
    {
        public override double ScoreMultiplier => 1.12;

        private const float default_flashlight_size = 180;

        private OsuFlashlight flashlight;

        public override Flashlight CreateFlashlight() => flashlight = new OsuFlashlight();

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var s in drawables.OfType<DrawableSlider>())
            {
                s.Tracking.ValueChanged += flashlight.OnSliderTrackingChange;
            }
        }

        private class OsuFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            public OsuFlashlight()
            {
                FlashlightSize = new Vector2(0, getSizeFor(0));
            }

            public void OnSliderTrackingChange(ValueChangedEvent<bool> e)
            {
                // If a slider is in a tracking state, a further dim should be applied to the (remaining) visible portion of the playfield over a brief duration.
                this.TransformTo(nameof(FlashlightDim), e.NewValue ? 0.8f : 0.0f, 50);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                const double follow_delay = 120;

                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    MathHelper.Clamp(Clock.ElapsedFrameTime, 0, follow_delay), position, destination, 0, follow_delay, Easing.Out);

                return base.OnMouseMove(e);
            }

            private float getSizeFor(int combo)
            {
                if (combo > 200)
                    return default_flashlight_size * 0.8f;
                else if (combo > 100)
                    return default_flashlight_size * 0.9f;
                else
                    return default_flashlight_size;
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, getSizeFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
