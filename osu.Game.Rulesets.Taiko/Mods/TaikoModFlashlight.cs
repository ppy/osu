// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModFlashlight : ModFlashlight<TaikoHitObject>
    {
        public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
        {
            MinValue = 0.5f,
            MaxValue = 1.5f,
            Precision = 0.1f
        };

        public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

        public override float DefaultFlashlightSize => 200;

        protected override Flashlight CreateFlashlight() => new TaikoFlashlight(this, DrawableRuleset);

        protected DrawableTaikoRuleset DrawableRuleset { get; private set; } = null!;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            DrawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        public partial class TaikoFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo);
            private readonly DrawableTaikoRuleset drawableRuleset;

            public TaikoFlashlight(TaikoModFlashlight modFlashlight, DrawableTaikoRuleset drawableRuleset)
                : base(modFlashlight)
            {
                this.drawableRuleset = drawableRuleset;

                FlashlightSize = new Vector2(0, GetSize());
                FlashlightSmoothness = 1.4f;

                AddLayout(flashlightProperties);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, size), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";

            protected override void Update()
            {
                base.Update();

                if (!flashlightProperties.IsValid)
                {
                    // https://github.com/peppy/osu-stable-reference/blob/baa8705f782c0de2b10a7387d78014c61c8b17fb/osu!/GameModes/Play/Rulesets/Taiko/RulesetTaiko.cs#L480-L481
                    // 1.6f is "magic factor" for matching stable positioning specs, see `OsuPlayfieldAdjustmentContainer` et al.
                    // the final factor is attempting to compensate for the aspect ratio clamping logic in `TaikoPlayfieldAdjustmentContainer`
                    // such that it does not change the visible range of objects.
                    FlashlightPosition = new Vector2(208 * 1.6f * drawableRuleset.PlayfieldAdjustmentContainer.Scale.X);

                    ClearTransforms(targetMember: nameof(FlashlightSize));
                    FlashlightSize = new Vector2(0, GetSize());

                    flashlightProperties.Validate();
                }
            }
        }
    }
}
