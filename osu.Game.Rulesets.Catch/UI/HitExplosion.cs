// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchHitExplosion : SkinnableDrawable
    {
        protected override bool ApplySizeRestrictionsToDefault => true;

        private CatchHitObject hitObject;

        public CatchHitObject HitObject
        {
            get => hitObject;
            set
            {
                hitObject = value;

                if (Drawable is ICatchHitExplosion hitExplosion)
                {
                    hitExplosion.HitObject = value;
                }
            }
        }

        public CatchHitExplosion()
            : base(new CatchSkinComponent(CatchSkinComponents.LightingGlow), _ => new DefaultHitExplosion())
        {
            RelativeSizeAxes = Axes.None;

            Size = new Vector2(20);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.BottomCentre;
        }

        private Color4 objectColour;

        public Color4 ObjectColour
        {
            get => objectColour;
            set
            {
                if (objectColour == value)
                    return;

                objectColour = value;

                if (Drawable is ICatchHitExplosion hitExplosion)
                {
                    hitExplosion.ObjectColour = value;
                }
            }
        }
    }

    public class DefaultHitExplosion : PoolableDrawable, ICatchHitExplosion
    {
        private readonly FadePiece largeFaint;
        private readonly FadePiece smallFaint;
        private readonly GlowPiece directionalGlow1;
        private readonly GlowPiece directionalGlow2;

        public DefaultHitExplosion()
        {
            Size = new Vector2(20);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.BottomCentre;

            const float initial_height = 10;

            InternalChildren = new Drawable[]
            {
                directionalGlow1 = new GlowPiece()
                {
                    Size = new Vector2(0.01f, initial_height),
                },
                directionalGlow2 = new GlowPiece()
                {
                    Size = new Vector2(0.01f, initial_height),
                },
                largeFaint = new FadePiece(),
                smallFaint = new FadePiece()
            };
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            const double duration = 400;

            largeFaint.Size = new Vector2(0.8f);
            largeFaint
                .ResizeTo(largeFaint.Size * new Vector2(5, 1), duration, Easing.OutQuint)
                .FadeOut(duration * 2);

            const float angle_variangle = 15; // should be less than 45

            directionalGlow1.Rotation = RNG.NextSingle(-angle_variangle, angle_variangle);
            directionalGlow2.Rotation = RNG.NextSingle(-angle_variangle, angle_variangle);

            this.FadeInFromZero(50).Then().FadeOut(duration, Easing.Out);

            Expire(true);
        }


        private void onColourChanged()
        {
            const float roundness = 100;

            largeFaint.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.1f, objectColour, Color4.White, 0, 1).Opacity(0.3f),
                Roundness = 160,
                Radius = 200,
            };

            smallFaint.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.6f, objectColour, Color4.White, 0, 1),
                Roundness = 20,
                Radius = 50,
            };

            directionalGlow1.EdgeEffect = directionalGlow2.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.4f, objectColour, Color4.White, 0, 1),
                Roundness = roundness,
                Radius = 40,
            };
        }

        private Color4 objectColour;

        public Color4 ObjectColour
        {
            get => objectColour;
            set
            {
                // We know this is a different value because we would
                // not be sent the update if it wasn't
                objectColour = value;

                onColourChanged();
            }
        }

        public CatchHitObject HitObject { get; set; }
    }
}
