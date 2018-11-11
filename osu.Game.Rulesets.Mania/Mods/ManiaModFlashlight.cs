// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFlashlight : ModFlashlight<ManiaHitObject>
    {
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModHidden) };

        private const float default_flashlight_size = 180;

        public override Flashlight CreateFlashlight() => new ManiaFlashlight();

        private class ManiaFlashlight : Flashlight
        {
            public ManiaFlashlight()
            {
                MousePosWrapper.Rectangular = true;
                MousePosWrapper.RectangularFlashlightSize = new Vector2(0, default_flashlight_size);
            }

            public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
            {
                if ((invalidation & Invalidation.DrawSize) > 0)
                {
                    Schedule(() =>
                    {
                        MousePosWrapper.RectangularFlashlightSize.X = DrawWidth;
                        MousePosWrapper.RectangularFlashlightSizeChanged = true;

                        MousePosWrapper.FlashlightPosition = ScreenSpaceDrawQuad.Centre;
                        MousePosWrapper.FlashlightPositionChanged = true;
                    });
                }

                return base.Invalidate(invalidation, source, shallPropagate);
            }

            protected override void OnComboChange(int newCombo)
            {
            }

            protected override void LoadComplete()
            {
                MousePosWrapper.RectangularFlashlightSize.X = DrawWidth;
                MousePosWrapper.RectangularFlashlightSizeChanged = true;

                MousePosWrapper.FlashlightPosition = ScreenSpaceDrawQuad.Centre;
                MousePosWrapper.FlashlightPositionChanged = true;
            }
        }
    }
}
