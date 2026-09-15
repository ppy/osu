// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModFlashlight : ModFlashlight<CatchHitObject>
    {
        public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
        {
            MinValue = 0.5f,
            MaxValue = 1.5f,
            Precision = 0.1f
        };

        public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

        public override float DefaultFlashlightSize => 203.125f;

        protected override Flashlight CreateFlashlight() => new CatchFlashlight(this, playfield);

        private CatchPlayfield playfield = null!;

        public override void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            playfield = (CatchPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private partial class CatchFlashlight : Flashlight
        {
            private readonly CatchPlayfield playfield;

            public CatchFlashlight(CatchModFlashlight modFlashlight, CatchPlayfield playfield)
                : base(modFlashlight)
            {
                this.playfield = playfield;

                FlashlightSize = new Vector2(0, GetSize());
                FlashlightSmoothness = 1.4f;
            }

            // all sizings here match stable as per
            // https://github.com/peppy/osu-stable-reference/blob/baa8705f782c0de2b10a7387d78014c61c8b17fb/osu!/GameModes/Play/Rulesets/Fruits/RulesetFruits.cs#L645-L655
            // all "scale" quantities below are relative to the `targetScale` of the flashlight effect at 0 combo

            protected override float BreakTimeScale => 1.538f; // ≈ 8.0 / 5.2

            protected override float GetComboScaleFor(int combo)
            {
                if (combo >= 200)
                    return 0.770f; // ≈ 4.0 / 5.2
                if (combo >= 100)
                    return 0.885f; // ≈ 4.6 / 5.2

                return 1.0f;
            }

            protected override void Update()
            {
                base.Update();

                FlashlightPosition = playfield.CatcherArea.ToSpaceOfOtherDrawable(playfield.Catcher.DrawPosition, this);
            }

            // as per https://github.com/peppy/osu-stable-reference/blob/baa8705f782c0de2b10a7387d78014c61c8b17fb/osu!/GameModes/Play/Rulesets/Fruits/RulesetFruits.cs#L657-L660,
            // stable's animation speed is 0.1 "units" per 1 frame at 60 fps
            // converting to local units here, this is:
            // (0.1 / 5.2) * (1 / 60 [s]) = 1.154 [1 / s] = (1.154 / 1000) [1 / ms]
            private const double scale_animation_speed = 1.154 / 1000;

            protected override void UpdateFlashlightSize(float size)
            {
                double relativeDelta = Math.Abs(FlashlightSize.Y - size) / DefaultFlashlightSize;
                double duration = relativeDelta / scale_animation_speed;
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, size), duration);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
