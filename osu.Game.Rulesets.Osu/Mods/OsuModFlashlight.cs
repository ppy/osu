// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

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
                FlashlightSize = new Vector2(0, getSizeFor(0));
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
