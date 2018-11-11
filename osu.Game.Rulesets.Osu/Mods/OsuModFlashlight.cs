// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlashlight : ModFlashlight<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        private const float default_flashlight_size = 180;

        public override Flashlight CreateFlashlight() => new OsuFlashlight();

        private class OsuFlashlight : Flashlight, IRequireHighFrequencyMousePosition
        {
            public OsuFlashlight()
            {
                MousePosWrapper.CircularFlashlightSize = getSizeFor(0);
                MousePosWrapper.Rectangular = false;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                MousePosWrapper.FlashlightPosition = e.ScreenSpaceMousePosition;
                MousePosWrapper.FlashlightPositionChanged = true;
                return base.OnMouseMove(e);
            }

            [UsedImplicitly]
            private float flashlightSize
            {
                set
                {
                    if (MousePosWrapper.CircularFlashlightSize == value) return;

                    MousePosWrapper.CircularFlashlightSize = value;
                    MousePosWrapper.CircularFlashlightSizeChanged = true;
                }

                get => MousePosWrapper.CircularFlashlightSize;
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

            protected override void OnComboChange(int newCombo)
            {
                this.TransformTo(nameof(flashlightSize), getSizeFor(newCombo), FLASHLIGHT_FADE_DURATION);
            }
        }
    }
}
