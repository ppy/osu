// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonHoldNoteTailPiece : CompositeDrawable
    {
        [Resolved]
        private DrawableHitObject? drawableObject { get; set; }

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        private readonly Box foreground;
        private readonly ArgonHoldNoteHittingLayer hittingLayer;
        private readonly Box foregroundAdditive;

        public ArgonHoldNoteTailPiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = ArgonNotePiece.NOTE_HEIGHT;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ArgonNotePiece.NOTE_HEIGHT,
                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Colour4.Black),
                            // Avoid ugly single pixel overlap.
                            Height = 0.9f,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Height = ArgonNotePiece.NOTE_ACCENT_RATIO,
                            CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                foreground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                hittingLayer = new ArgonHoldNoteHittingLayer(),
                                foregroundAdditive = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Blending = BlendingParameters.Additive,
                                    Height = 0.5f,
                                },
                            },
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            if (drawableObject != null)
            {
                accentColour.BindTo(drawableObject.AccentColour);
                accentColour.BindValueChanged(onAccentChanged, true);

                drawableObject.HitObjectApplied += hitObjectApplied;
            }
        }

        private void hitObjectApplied(DrawableHitObject drawableHitObject)
        {
            var holdNoteTail = (DrawableHoldNoteTail)drawableHitObject;

            hittingLayer.Recycle();

            hittingLayer.AccentColour.UnbindBindings();
            hittingLayer.AccentColour.BindTo(holdNoteTail.HoldNote.AccentColour);

            hittingLayer.IsHitting.UnbindBindings();
            ((IBindable<bool>)hittingLayer.IsHitting).BindTo(holdNoteTail.HoldNote.IsHitting);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            Scale = new Vector2(1, direction.NewValue == ScrollingDirection.Up ? -1 : 1);
        }

        private void onAccentChanged(ValueChangedEvent<Color4> accent)
        {
            foreground.Colour = accent.NewValue.Darken(0.6f); // matches body

            foregroundAdditive.Colour = ColourInfo.GradientVertical(
                accent.NewValue.Opacity(0.4f),
                accent.NewValue.Opacity(0)
            );
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject != null)
                drawableObject.HitObjectApplied -= hitObjectApplied;
        }
    }
}
