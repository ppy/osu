// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class DefaultHitExplosion : CompositeDrawable, IHitExplosion
    {
        private const float default_large_faint_size = 0.8f;

        public override bool RemoveWhenNotAlive => true;

        [Resolved]
        private Column column { get; set; }

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private CircularContainer largeFaint;
        private CircularContainer mainGlow1;
        private CircularContainer mainGlow2;
        private CircularContainer mainGlow3;

        private Bindable<Color4> accentColour;

        public DefaultHitExplosion()
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            Height = DefaultNotePiece.NOTE_HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            const float angle_variance = 15; // should be less than 45
            const float roundness = 80;
            const float initial_height = 10;

            InternalChildren = new Drawable[]
            {
                largeFaint = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    // we want our size to be very small so the glow dominates it.
                    Size = new Vector2(default_large_faint_size),
                    Blending = BlendingParameters.Additive,
                },
                mainGlow1 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                },
                mainGlow2 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variance, angle_variance),
                },
                mainGlow3 = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variance, angle_variance),
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            accentColour = column.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(colour =>
            {
                largeFaint.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.1f, colour.NewValue, Color4.White, 0, 1).Opacity(0.3f),
                    Roundness = 160,
                    Radius = 200,
                };
                mainGlow1.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.6f, colour.NewValue, Color4.White, 0, 1),
                    Roundness = 20,
                    Radius = 50,
                };
                mainGlow2.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.4f, colour.NewValue, Color4.White, 0, 1),
                    Roundness = roundness,
                    Radius = 40,
                };
                mainGlow3.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.4f, colour.NewValue, Color4.White, 0, 1),
                    Roundness = roundness,
                    Radius = 40,
                };
            }, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                Anchor = Anchor.TopCentre;
                Y = DefaultNotePiece.NOTE_HEIGHT / 2;
            }
            else
            {
                Anchor = Anchor.BottomCentre;
                Y = -DefaultNotePiece.NOTE_HEIGHT / 2;
            }
        }

        public void Animate(Judgement result)
        {
            // scale roughly in-line with visual appearance of notes
            Vector2 scale = new Vector2(1, 0.6f);

            this.ScaleTo(scale);

            largeFaint
                .ResizeTo(default_large_faint_size)
                .Then()
                .ResizeTo(default_large_faint_size * new Vector2(5, 1), PoolableHitExplosion.DURATION, Easing.OutQuint)
                .FadeOut(PoolableHitExplosion.DURATION * 2);

            mainGlow1
                .ScaleTo(1)
                .Then()
                .ScaleTo(1.4f, PoolableHitExplosion.DURATION, Easing.OutQuint);

            this.FadeOutFromOne(PoolableHitExplosion.DURATION, Easing.Out);
        }
    }
}
