// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFlashlight : ModFlashlight<CatchHitObject>
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        public CatchModFlashlight()
        {
            StartingFlashlightSize.MaxValue = 1.5f;
        }

        public override float DefaultFlashlightSize => 325;

        protected override Flashlight CreateFlashlight() => new CatchFlashlight(this, playfield);

        private CatchPlayfield playfield = null!;

        public override void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            FinalFlashlightSize.Default = 0.77f;

            playfield = (CatchPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private class CatchFlashlight : Flashlight
        {
            private readonly CatchPlayfield playfield;

            public CatchFlashlight(CatchModFlashlight modFlashlight, CatchPlayfield playfield)
                : base(modFlashlight)
            {
                this.playfield = playfield;

                FlashlightSize = new Vector2(0, GetSize());
                FlashlightSmoothness = 1.4f;
            }

            protected override void Update()
            {
                base.Update();

                FlashlightPosition = playfield.CatcherArea.ToSpaceOfOtherDrawable(playfield.Catcher.DrawPosition, this);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, size), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
