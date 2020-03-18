// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.UI
{
    public class KiaiHitExplosion : CompositeDrawable
    {
        public override bool RemoveWhenNotAlive => true;

        private readonly CircularContainer largeFaint;
        private readonly CircularContainer mainGlow1;

        public KiaiHitExplosion(DrawableHitObject judgedObject, bool isSmall = false)
        {
            Height = 10;
            Width = 10;

            // scale roughly in-line with visual appearance of notes
            Scale = new Vector2(1f, 0.5f);

            if (isSmall)
                Scale *= 0.5f;

            const float angle_variangle = 15; // should be less than 45

            const float roundness = 80;

            const float initial_height = 10;

            var colour = Interpolation.ValueAt(0.4f, judgedObject.AccentColour.Value, Color4.White, 0, 1);

            InternalChildren = new Drawable[]
            {
                largeFaint = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    // we want our size to be very small so the glow dominates it.
                    Size = new Vector2(0.8f),
                    Blending = BlendingParameters.Additive,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Interpolation.ValueAt(0.1f, judgedObject.AccentColour.Value, Color4.White, 0, 1).Opacity(0.3f),
                        Roundness = 160,
                        Radius = 200,
                    },
                },
                mainGlow1 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Interpolation.ValueAt(0.6f, judgedObject.AccentColour.Value, Color4.White, 0, 1),
                        Roundness = 20,
                        Radius = 50,
                    },
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variangle, angle_variangle),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = colour,
                        Roundness = roundness,
                        Radius = 40,
                    },
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variangle, angle_variangle),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = colour,
                        Roundness = roundness,
                        Radius = 40,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            const double duration = 200;

            base.LoadComplete();

            largeFaint
                .ResizeTo(largeFaint.Size * new Vector2(5, 1), duration, Easing.OutQuint)
                .FadeOut(duration * 2);

            mainGlow1.ScaleTo(1.4f, duration, Easing.OutQuint);

            this.FadeOut(duration, Easing.Out);
            Expire(true);
        }
    }
}
