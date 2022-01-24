// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFlashlight : ModFlashlight<CatchHitObject>
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
            MinValue = 0.4f,
            MaxValue = 1.7f,
            Default = 1f,
            Value = 1f,
            Precision = 0.1f
        };

        protected override float ModeMultiplier => 350;

        public override Flashlight CreateFlashlight() => new CatchFlashlight(playfield, ChangeRadius.Value, InitialRadius.Value, ModeMultiplier);

        private CatchPlayfield playfield;

        public override void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            playfield = (CatchPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private class CatchFlashlight : Flashlight
        {
            private readonly CatchPlayfield playfield;

            public CatchFlashlight(CatchPlayfield playfield, bool isRadiusBasedOnCombo, float initialRadius, float modeMultiplier)
                : base(isRadiusBasedOnCombo, initialRadius, modeMultiplier)
            {
                this.playfield = playfield;
                FlashlightSize = new Vector2(0, GetRadiusFor(0));
            }

            protected override void Update()
            {
                base.Update();

                FlashlightPosition = playfield.CatcherArea.ToSpaceOfOtherDrawable(playfield.Catcher.DrawPosition, this);
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, GetRadiusFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
