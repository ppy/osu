// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
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
                    Colour = ColourInfo.GradientVertical(
                        new Color4(255, 227, 236, 255),
                        new Color4(255, 198, 211, 255)
                    ),
                    Masking = true,
                },
                new Circle
                {
                    Name = "Inner circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new Vector2(0.85f),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(255, 132, 191, 255).Opacity(0.5f),
                        Radius = 45,
                    },
                    Masking = true,
                },
            };
        }

        public void Animate(DrawableHitObject drawableHitObject)
        {
            this.FadeOut();

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
