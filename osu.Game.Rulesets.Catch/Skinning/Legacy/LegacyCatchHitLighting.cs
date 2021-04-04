// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatchHitLighting : CatchHitExplosion
    {
        private readonly Sprite explosion1;
        private readonly Sprite explosion2;

        [Resolved]
        protected ISkinSource Skin { get; private set; }

        public LegacyCatchHitLighting(Texture explosionTexture1, Texture explosionTexture2)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                explosion1 = new Sprite()
                {
                    Origin = Anchor.CentreLeft,
                    Texture = explosionTexture1,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90,
                },
                explosion2 = new Sprite()
                {
                    Origin = Anchor.CentreLeft,
                    Texture = explosionTexture2,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90,
                }
            };
        }

        public override void Animate()
        {
            explosion1.Colour = explosion2.Colour = ObjectColour;

            float catcherWidthHalf = CatcherWidth * 0.5f;

            float explosionOffset = Math.Clamp(CatchPosition, -catcherWidthHalf + CatcherMargin * 3, catcherWidthHalf - CatcherMargin * 3);

            if (!(HitObject is Droplet))
            {
                var scale = Math.Clamp(JudgementResult.ComboAtJudgement / 200f, 0.35f, 1.125f);

                explosion1.Scale = new Vector2(1, 0.9f);
                explosion1.Alpha = 1;
                explosion1.Position = new Vector2(explosionOffset, 0);

                explosion1.FadeOut(300);
                explosion1.ScaleTo(new Vector2(16 * scale, 1.1f), 160, Easing.Out);
            }

            explosion2.Scale = new Vector2(0.9f, 1f);
            explosion2.Alpha = 1;
            explosion2.Position = new Vector2(explosionOffset, 0);

            explosion2.FadeOut(700);
            explosion2.ScaleTo(new Vector2(0.9f, 1.3f), 500, Easing.Out);

            this.FadeInFromZero(20).Then().FadeOut(1000, Easing.Out);

            Expire(true);
        }
    }
}
