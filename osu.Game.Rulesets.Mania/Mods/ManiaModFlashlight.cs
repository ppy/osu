// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osuTK;

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
            private readonly Cached flashlightProperties = new Cached();

            public ManiaFlashlight()
            {
                FlashlightSize = new Vector2(0, default_flashlight_size);
            }

            public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
            {
                if ((invalidation & Invalidation.DrawSize) > 0)
                {
                    flashlightProperties.Invalidate();
                }

                return base.Invalidate(invalidation, source, shallPropagate);
            }

            protected override void Update()
            {
                base.Update();

                if (!flashlightProperties.IsValid)
                {
                    FlashlightSize = new Vector2(DrawWidth, FlashlightSize.Y);

                    FlashlightPosition = DrawPosition + DrawSize / 2;
                    flashlightProperties.Validate();
                }
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
            }

            protected override string FragmentShader => "RectangularFlashlight";
        }
    }
}
