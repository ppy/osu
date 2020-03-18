// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Tau.Objects;
using osuTK;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModFlashlight : ModFlashlight<TauHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        private const float default_flashlight_size = 180;

        private TauFlashlight flashlight;

        public override Flashlight CreateFlashlight() => flashlight = new TauFlashlight();

        private class TauFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            public TauFlashlight()
            {
                FlashlightSize = new Vector2(0, getSizeFor(0));
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                const double follow_delay = 120;

                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    Math.Clamp(Clock.ElapsedFrameTime, 0, follow_delay), position, destination, 0, follow_delay, Easing.Out);

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
