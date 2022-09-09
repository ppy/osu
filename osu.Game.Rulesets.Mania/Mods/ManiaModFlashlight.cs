// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFlashlight : ModFlashlight<ManiaHitObject>
    {
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModHidden) };

        [SettingSource("Flashlight size", "Multiplier applied to the default flashlight size.")]
        public override BindableFloat SizeMultiplier { get; } = new BindableFloat
        {
            MinValue = 0.5f,
            MaxValue = 3f,
            Default = 1f,
            Value = 1f,
            Precision = 0.1f
        };

        [SettingSource("Change size based on combo", "Decrease the flashlight size as combo increases.")]
        public override BindableBool ComboBasedSize { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public override float DefaultFlashlightSize => 50;

        protected override Flashlight CreateFlashlight() => new ManiaFlashlight(this);

        private class ManiaFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.DrawSize);

            public ManiaFlashlight(ManiaModFlashlight modFlashlight)
                : base(modFlashlight)
            {
                FlashlightSize = new Vector2(DrawWidth, GetSizeFor(0));

                AddLayout(flashlightProperties);
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
                this.TransformTo(nameof(FlashlightSize), new Vector2(DrawWidth, GetSizeFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "RectangularFlashlight";
        }
    }
}
