// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DefaultHitExplosion : CompositeDrawable, IHitExplosion
    {
        private const float default_large_faint_size = 0.8f;

        public override bool RemoveWhenNotAlive => true;

        [Resolved]
        private Column column { get; set; }

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private CircularContainer largeFaint;
        private CircularContainer mainGlow1;

        public DefaultHitExplosion()
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            Height = DefaultNotePiece.NOTE_HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            const float angle_variangle = 15; // should be less than 45
            const float roundness = 80;
            const float initial_height = 10;

            var colour = Interpolation.ValueAt(0.4f, column.AccentColour, Color4.White, 0, 1);

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
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Interpolation.ValueAt(0.1f, column.AccentColour, Color4.White, 0, 1).Opacity(0.3f),
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
                        Colour = Interpolation.ValueAt(0.6f, column.AccentColour, Color4.White, 0, 1),
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

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
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

        public void Animate(JudgementResult result)
        {
            // scale roughly in-line with visual appearance of notes
            Vector2 scale = new Vector2(1, 0.6f);

            if (result.Judgement is HoldNoteTickJudgement)
                scale *= 0.5f;

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
