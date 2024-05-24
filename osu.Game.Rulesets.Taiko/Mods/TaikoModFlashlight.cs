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
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
        {
            MinValue = 0.5f,
            MaxValue = 1.5f,
            Precision = 0.1f
        };

        public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

        public override float DefaultFlashlightSize => 200;

        protected override Flashlight CreateFlashlight() => new TaikoFlashlight(this, Playfield);

        protected TaikoPlayfield Playfield { get; private set; } = null!;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            Playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        public partial class TaikoFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo);
            private readonly TaikoPlayfield taikoPlayfield;

            public TaikoFlashlight(TaikoModFlashlight modFlashlight, TaikoPlayfield taikoPlayfield)
                : base(modFlashlight)
            {
                this.taikoPlayfield = taikoPlayfield;

                FlashlightSize = adjustSizeForPlayfieldAspectRatio(GetSize());
                FlashlightSmoothness = 1.4f;

                AddLayout(flashlightProperties);
            }

            /// <summary>
            /// Returns the aspect ratio-adjusted size of the flashlight.
            /// This ensures that the size of the flashlight remains independent of taiko-specific aspect ratio adjustments.
            /// </summary>
            /// <param name="size">
            /// The size of the flashlight.
            /// The value provided here should always come from <see cref="ModFlashlight{T}.Flashlight.GetSize"/>.
            /// </param>
            private Vector2 adjustSizeForPlayfieldAspectRatio(float size)
            {
                return new Vector2(0, size * taikoPlayfield.Parent!.Scale.Y);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), adjustSizeForPlayfieldAspectRatio(size), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";

            protected override void Update()
            {
                base.Update();

                if (!flashlightProperties.IsValid)
                {
                    FlashlightPosition = ToLocalSpace(taikoPlayfield.HitTarget.ScreenSpaceDrawQuad.Centre);

                    ClearTransforms(targetMember: nameof(FlashlightSize));
                    FlashlightSize = adjustSizeForPlayfieldAspectRatio(GetSize());

                    flashlightProperties.Validate();
                }
            }
        }
    }
}
