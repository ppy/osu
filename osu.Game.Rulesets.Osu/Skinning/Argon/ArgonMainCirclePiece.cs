// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
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
                        const double fade_out_time = 600;
                        const double flash_in = 200;

                        number.FadeOut(flash_in / 4);

                        outerGradient.ResizeTo(Size, fade_out_time / 2, Easing.OutQuint);
                        outerGradient.FadeOut(flash_in);

                        innerGradient.ResizeTo(Size, fade_out_time * 2, Easing.OutQuint);
                        innerGradient.FadeOut(flash_in);

                        innerFill.ResizeTo(Size, fade_out_time * 1.5, Easing.OutQuint);
                        innerFill.FadeOut(flash_in);

                        outerFill.FadeOut(flash_in);

                        ring.ResizeTo(Size + new Vector2(ring.BorderThickness * 1.5f), fade_out_time / 1.5f, Easing.OutQuint);
                        ring.FadeOut(fade_out_time);

                        using (BeginDelayedSequence(flash_in))
                            this.FadeOut(fade_out_time, Easing.OutQuint);

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
    }
}
