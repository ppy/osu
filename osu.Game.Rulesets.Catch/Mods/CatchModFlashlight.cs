// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFlashlight : ModFlashlight<CatchHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        private const float default_flashlight_size = 350;

        public override Flashlight CreateFlashlight() => new CatchFlashlight(playfield);

        private CatchPlayfield playfield;

        public override void ApplyToRulesetContainer(RulesetContainer<CatchHitObject> rulesetContainer)
        {
            playfield = (CatchPlayfield)rulesetContainer.Playfield;
            base.ApplyToRulesetContainer(rulesetContainer);
        }

        private class CatchFlashlight : Flashlight
        {
            private readonly CatchPlayfield playfield;

            public CatchFlashlight(CatchPlayfield playfield)
            {
                this.playfield = playfield;
                MousePosWrapper.CircularFlashlightSize = getSizeFor(0);
                MousePosWrapper.Rectangular = false;
            }

            protected override void Update()
            {
                base.Update();

                MousePosWrapper.FlashlightPosition = (playfield.CatcherArea.MovableCatcher.ScreenSpaceDrawQuad.TopLeft + playfield.CatcherArea.MovableCatcher.ScreenSpaceDrawQuad.TopRight) / 2;
                MousePosWrapper.FlashlightPositionChanged = true;
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
