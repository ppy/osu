// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModFlashlight : ModFlashlight<TaikoHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        [SettingSource("Change radius based on combo", "Decrease the flashlight radius as combo increases.")]
        public override BindableBool ChangeRadius { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Initial radius", "Initial radius of the flashlight area.")]
        public override BindableNumber<float> InitialRadius { get; } = new BindableNumber<float>
        {
            MinValue = 0,
            MaxValue = 1.66f,
            Default = 1f,
            Value = 1f,
            Precision = 0.1f
        };

        protected override float ModeMultiplier => 250;

        public override Flashlight CreateFlashlight() => new TaikoFlashlight(playfield, ChangeRadius.Value, InitialRadius.Value, ModeMultiplier);

        private TaikoPlayfield playfield;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private class TaikoFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.DrawSize);
            private readonly TaikoPlayfield taikoPlayfield;

            public TaikoFlashlight(TaikoPlayfield taikoPlayfield, bool isRadiusBasedOnCombo, float initialRadius, float modeMultiplier)
                : base(isRadiusBasedOnCombo, initialRadius, modeMultiplier)
            {
                this.taikoPlayfield = taikoPlayfield;
                FlashlightSize = getSizeFor(0);

                AddLayout(flashlightProperties);
            }

            private Vector2 getSizeFor(int combo)
            {
                // Preserve flashlight size through the playfield's aspect adjustment.
                return new Vector2(0, GetRadiusFor(combo) * taikoPlayfield.DrawHeight / TaikoPlayfield.DEFAULT_HEIGHT);
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), getSizeFor(e.NewValue), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";

            protected override void Update()
            {
                base.Update();

                if (!flashlightProperties.IsValid)
                {
                    FlashlightPosition = ToLocalSpace(taikoPlayfield.HitTarget.ScreenSpaceDrawQuad.Centre);

                    ClearTransforms(targetMember: nameof(FlashlightSize));
                    FlashlightSize = getSizeFor(Combo.Value);

                    flashlightProperties.Validate();
                }
            }
        }
    }
}
