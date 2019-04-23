// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
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
            private int trackingSliders;

            public OsuFlashlight()
            {
                FlashlightSize = new Vector2(0, getSizeFor(0));
            }

            public void OnSliderTrackingChange(ValueChangedEvent<bool> e)
            {
                // If any sliders are in a tracking state, apply a dim to the entire playfield over a brief duration.
                if (e.NewValue)
                {
                    trackingSliders++;
                    // The fade should only be applied if tracking sliders is increasing from 0 to 1, and cannot be a result of a slider losing tracking.
                    // As a result, this logic must be exclusive to when e.NewValue is true.
                    if (trackingSliders == 1)
                    {
                        this.TransformTo(nameof(FlashlightDim), 0.8f, 50);
                    }
                }
                else
                {
                    trackingSliders--;

                    if (trackingSliders == 0)
                    {
                        this.TransformTo(nameof(FlashlightDim), 0.0f, 50);
                    }
                }

                if (trackingSliders < 0)
                    throw new InvalidOperationException($"The number of {nameof(trackingSliders)} cannot be below 0.");
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                FlashlightPosition = e.MousePosition;
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
