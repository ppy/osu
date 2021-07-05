// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class HitExplosion : PoolableDrawableWithLifetime<HitExplosionEntry>
    {
        private readonly CircularContainer largeFaint;
        private readonly CircularContainer smallFaint;
        private readonly CircularContainer directionalGlow1;
        private readonly CircularContainer directionalGlow2;

        public HitExplosion()
        {
            Size = new Vector2(20);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.BottomCentre;

            // scale roughly in-line with visual appearance of notes
            const float initial_height = 10;

            InternalChildren = new Drawable[]
            {
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
                },
                directionalGlow1 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                },
                directionalGlow2 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                }
            };
        }

        protected override void OnApply(HitExplosionEntry entry)
        {
            X = entry.Position;
            Scale = new Vector2(entry.Scale);
            setColour(entry.ObjectColour);

            using (BeginAbsoluteSequence(entry.LifetimeStart))
                applyTransforms(entry.RNGSeed);
        }

        private void applyTransforms(int randomSeed)
        {
            ClearTransforms(true);

            const double duration = 400;

            // we want our size to be very small so the glow dominates it.
            largeFaint.Size = new Vector2(0.8f);
            largeFaint
                .ResizeTo(largeFaint.Size * new Vector2(5, 1), duration, Easing.OutQuint)
                .FadeOut(duration * 2);

            const float angle_variangle = 15; // should be less than 45
            directionalGlow1.Rotation = StatelessRNG.NextSingle(-angle_variangle, angle_variangle, randomSeed, 4);
            directionalGlow2.Rotation = StatelessRNG.NextSingle(-angle_variangle, angle_variangle, randomSeed, 5);

            this.FadeInFromZero(50).Then().FadeOut(duration, Easing.Out).Expire();
        }

        private void setColour(Color4 objectColour)
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
    }
}
