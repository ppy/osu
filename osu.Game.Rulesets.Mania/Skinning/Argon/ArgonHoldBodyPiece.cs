// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    public class ArgonHoldBodyPiece : CompositeDrawable, IHoldNoteBody
    {
        protected readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
        protected readonly IBindable<bool> IsHitting = new Bindable<bool>();

        private Drawable background = null!;
        private Box foreground = null!;

        public ArgonHoldBodyPiece()
        {
            RelativeSizeAxes = Axes.Both;

            // Without this, the width of the body will be slightly larger than the head/tail.
            Masking = true;
            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader(true)]
        private void load(DrawableHitObject? drawableObject)
        {
            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                foreground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            };

            if (drawableObject != null)
            {
                var holdNote = (DrawableHoldNote)drawableObject;

                AccentColour.BindTo(holdNote.AccentColour);
                IsHitting.BindTo(holdNote.IsHitting);
            }

            AccentColour.BindValueChanged(colour =>
            {
                background.Colour = colour.NewValue.Darken(1.2f);
                foreground.Colour = colour.NewValue.Opacity(0.2f);
            }, true);

            IsHitting.BindValueChanged(hitting =>
            {
                const float animation_length = 50;

                foreground.ClearTransforms();

                if (hitting.NewValue)
                {
                    // wait for the next sync point
                    double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);

                    using (foreground.BeginDelayedSequence(synchronisedOffset))
                    {
                        foreground.FadeTo(1, animation_length).Then()
                                  .FadeTo(0.5f, animation_length)
                                  .Loop();
                    }
                }
                else
                {
                    foreground.FadeOut(animation_length);
                }
            });
        }

        public void Recycle()
        {
            foreground.ClearTransforms();
            foreground.Alpha = 0;
        }
    }
}
