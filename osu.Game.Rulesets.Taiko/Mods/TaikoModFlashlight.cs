// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
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

        public override Flashlight CreateFlashlight() => new TaikoFlashlight(playfield);

        private TaikoPlayfield playfield;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private class TaikoFlashlight : Flashlight
        {
            private readonly Cached flashlightProperties = new Cached();
            private readonly TaikoPlayfield taikoPlayfield;

            public TaikoFlashlight(TaikoPlayfield taikoPlayfield)
            {
                this.taikoPlayfield = taikoPlayfield;
                FlashlightSize = new Vector2(0, getSizeFor(0));
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
                    FlashlightPosition = taikoPlayfield.HitTarget.ToSpaceOfOtherDrawable(taikoPlayfield.HitTarget.OriginPosition, this);
                    flashlightProperties.Validate();
                }
            }
        }
    }
}
