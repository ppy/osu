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

        private const float default_flashlight_size = 250;

        private const int max_combo_offset = 200;

        private TaikoFlashlight flashlight;

        public override Flashlight CreateFlashlight() => flashlight = new TaikoFlashlight(playfield);

        private TaikoPlayfield playfield;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);

            flashlight.ComboOffset = ComboOffset.Value;
        }

        [SettingSource("Combo offset", "Combo to start at for changing flashlight radius")]
        public BindableNumber<int> ComboOffset { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = max_combo_offset,
            Precision = 1,
        };

        private class TaikoFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.DrawSize);
            private readonly TaikoPlayfield taikoPlayfield;

            public int ComboOffset { private get; set; }

            public TaikoFlashlight(TaikoPlayfield taikoPlayfield)
            {
                this.taikoPlayfield = taikoPlayfield;
                FlashlightSize = new Vector2(0, getSizeFor(0));

                AddLayout(flashlightProperties);
            }

            private Vector2 getSizeFor(int combo)
            {
                int effectiveCombo = combo + ComboOffset;

                float size = default_flashlight_size;

                if (effectiveCombo > 200)
                    size *= 0.8f;
                else if (effectiveCombo > 100)
                    size *= 0.9f;

                // Preserve flashlight size through the playfield's aspect adjustment.
                return new Vector2(0, size * taikoPlayfield.DrawHeight / TaikoPlayfield.DEFAULT_HEIGHT);
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, getSizeFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";

            protected override void Update()
            {
                base.Update();

                if (!flashlightProperties.IsValid)
                {
                    FlashlightPosition = taikoPlayfield.HitTarget.ToSpaceOfOtherDrawable(taikoPlayfield.HitTarget.OriginPosition, this);
                    flashlightProperties.Validate();
                }
            }
        }
    }
}
