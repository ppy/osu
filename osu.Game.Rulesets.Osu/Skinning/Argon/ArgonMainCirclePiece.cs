// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class ArgonMainCirclePiece : CompositeDrawable
    {
        private readonly Circle outerFill;
        private readonly Circle outerGradient;
        private readonly Circle innerGradient;
        private readonly Circle innerFill;

        private readonly RingPiece ring;
        private readonly OsuSpriteText number;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
        private readonly IBindable<int> indexInCurrentCombo = new Bindable<int>();
        private readonly FlashPiece flash;

        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        public ArgonMainCirclePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            const float border_thickness = 7;
            const float fill_thickness = 24;

            InternalChildren = new Drawable[]
            {
                outerFill = new Circle // renders white outer border and dark fill
                {
                    Size = Size,
                    Alpha = 1,
                },
                outerGradient = new Circle // renders the outer bright gradient
                {
                    Size = outerFill.Size - new Vector2(border_thickness * 3),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                innerGradient = new Circle // renders the inner bright gradient
                {
                    Size = outerGradient.Size - new Vector2(fill_thickness),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                innerFill = new Circle // renders the inner dark fill
                {
                    Size = innerGradient.Size - new Vector2(fill_thickness),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                number = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 52, weight: FontWeight.Bold),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -2,
                    Text = @"1",
                },
                flash = new FlashPiece(),
                ring = new RingPiece(border_thickness),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var drawableOsuObject = (DrawableOsuHitObject)drawableObject;

            accentColour.BindTo(drawableObject.AccentColour);
            indexInCurrentCombo.BindTo(drawableOsuObject.IndexInCurrentComboBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindValueChanged(colour =>
            {
                outerFill.Colour = innerFill.Colour = colour.NewValue.Darken(4);
                outerGradient.Colour = ColourInfo.GradientVertical(colour.NewValue, colour.NewValue.Darken(0.1f));
                innerGradient.Colour = ColourInfo.GradientVertical(colour.NewValue.Darken(0.5f), colour.NewValue.Darken(0.6f));
                flash.Colour = colour.NewValue.Multiply(1f);
            }, true);

            indexInCurrentCombo.BindValueChanged(index => number.Text = (index.NewValue + 1).ToString(), true);

            drawableObject.ApplyCustomUpdateState += updateStateTransforms;
            updateStateTransforms(drawableObject, drawableObject.State.Value);
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        const double fade_out_time = 800;
                        const double flash_in = 150;

                        number.FadeOut(flash_in / 2);

                        outerFill.FadeOut(flash_in);

                        ring.ResizeTo(Size - new Vector2(ring.BorderThickness * 1.5f), flash_in, Easing.OutQuint);

                        ring.TransformTo(nameof
                            (BorderColour), ColourInfo.GradientVertical(
                            accentColour.Value.Opacity(0.5f),
                            accentColour.Value.Opacity(0)), fade_out_time);

                        flash.FadeTo(1, flash_in * 2, Easing.OutQuint);

                        using (BeginDelayedSequence(flash_in / 8))
                        {
                            outerGradient.ResizeTo(outerGradient.Size * 0.8f, flash_in, Easing.OutQuint);

                            using (BeginDelayedSequence(flash_in / 8))
                            {
                                innerGradient.ResizeTo(innerGradient.Size * 0.8f, flash_in, Easing.OutQuint);
                                innerFill.ResizeTo(innerFill.Size * 0.8f, flash_in, Easing.OutQuint);
                            }
                        }

                        innerFill.FadeOut(flash_in, Easing.OutQuint);
                        innerGradient.FadeOut(flash_in, Easing.OutQuint);

                        this.FadeOut(fade_out_time, Easing.OutQuad);

                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject.IsNotNull())
                drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }

        private class FlashPiece : Circle
        {
            public FlashPiece()
            {
                Size = new Vector2(OsuHitObject.OBJECT_RADIUS);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Alpha = 0;
                Blending = BlendingParameters.Additive;

                Child.Alpha = 0;
                Child.AlwaysPresent = true;
            }

            protected override void Update()
            {
                base.Update();
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Colour,
                    Radius = OsuHitObject.OBJECT_RADIUS * 1.4f,
                };
            }
        }
    }
}
