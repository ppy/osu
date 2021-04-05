// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class DefaultHitExplosion : CatchHitExplosion
    {
        private readonly CircularContainer largeFaint;
        private readonly CircularContainer smallFaint;
        private readonly CircularContainer directionalGlow1;
        private readonly CircularContainer directionalGlow2;

        private Color4 lastColor;

        public DefaultHitExplosion()
        {
            Size = new Vector2(20);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.BottomCentre;

            const float initial_height = 10;

            InternalChildren = new Drawable[]
            {
                directionalGlow1 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                    Size = new Vector2(0.01f, initial_height)
                },
                directionalGlow2 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                    Size = new Vector2(0.01f, initial_height),
                },
                largeFaint = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                },
                smallFaint = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                }
            };
        }

        private void onColourChanged()
        {
            const float roundness = 100;

            largeFaint.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.1f, ObjectColour, Color4.White, 0, 1).Opacity(0.3f),
                Roundness = 160,
                Radius = 200,
            };

            smallFaint.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.6f, ObjectColour, Color4.White, 0, 1),
                Roundness = 20,
                Radius = 50,
            };

            directionalGlow1.EdgeEffect = directionalGlow2.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Interpolation.ValueAt(0.4f, ObjectColour, Color4.White, 0, 1),
                Roundness = roundness,
                Radius = 40,
            };
        }

        public override void Animate()
        {
            Scale = new Vector2(HitObject.Scale);

            const double duration = 400;

            // If the color has changed since last time this was animated
            // or has never been assigned.
            if (lastColor != ObjectColour)
            {
                lastColor = ObjectColour;
                onColourChanged();
            }

            X = CatchPosition;

            largeFaint.Size = new Vector2(0.8f);
            largeFaint
                .ResizeTo(largeFaint.Size * new Vector2(5, 1), duration, Easing.OutQuint)
                .FadeOutFromOne(duration * 2);

            const float angle_variangle = 15; // should be less than 45

            directionalGlow1.Rotation = RNG.NextSingle(-angle_variangle, angle_variangle);
            directionalGlow2.Rotation = RNG.NextSingle(-angle_variangle, angle_variangle);

            this.FadeInFromZero(50).Then().FadeOut(duration, Easing.Out);
        }
    }
}
