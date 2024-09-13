// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonHitExplosion : CompositeDrawable, IAnimatableHitExplosion
    {
        private readonly TaikoSkinComponents component;

        private readonly Circle outer;
        private readonly Circle inner;

        public ArgonHitExplosion(TaikoSkinComponents component)
        {
            this.component = component;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                outer = new Circle
                {
                    Name = "Outer circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                },
                inner = new Circle
                {
                    Name = "Inner circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new Vector2(0.85f),
                    Masking = true,
                },
            };
        }

        public void Animate(DrawableHitObject drawableHitObject)
        {
            this.FadeOut();

            bool isRim = drawableHitObject.HitObject is Hit { Type: HitType.Rim };

            outer.Colour = isRim ? ArgonInputDrum.RIM_HIT_GRADIENT : ArgonInputDrum.CENTRE_HIT_GRADIENT;
            inner.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = (isRim ? ArgonInputDrum.RIM_HIT_GLOW : ArgonInputDrum.CENTRE_HIT_GLOW).Opacity(0.5f),
                Radius = 45,
            };

            switch (component)
            {
                case TaikoSkinComponents.TaikoExplosionGreat:
                    this.FadeIn(30, Easing.In)
                        .Then()
                        .FadeOut(450, Easing.OutQuint);
                    break;

                case TaikoSkinComponents.TaikoExplosionOk:
                    this.FadeTo(0.2f, 30, Easing.In)
                        .Then()
                        .FadeOut(200, Easing.OutQuint);
                    break;
            }
        }

        public void AnimateSecondHit()
        {
            outer.ResizeTo(new Vector2(TaikoStrongableHitObject.STRONG_SCALE), 500, Easing.OutQuint);
        }
    }
}
