// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    public class DefaultBodyPiece : CompositeDrawable
    {
        protected readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        private readonly LayoutValue subtractionCache = new LayoutValue(Invalidation.DrawSize);
        private readonly IBindable<bool> isHitting = new Bindable<bool>();

        protected Drawable Background { get; private set; }
        protected BufferedContainer Foreground { get; private set; }

        private BufferedContainer subtractionContainer;
        private Container subtractionLayer;

        public DefaultBodyPiece()
        {
            Blending = BlendingParameters.Additive;

            AddLayout(subtractionCache);
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] DrawableHitObject drawableObject)
        {
            InternalChildren = new[]
            {
                Background = new Box { RelativeSizeAxes = Axes.Both },
                Foreground = new BufferedContainer
                {
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both },
                        subtractionContainer = new BufferedContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            // This is needed because we're blending with another object
                            BackgroundColour = Color4.White.Opacity(0),
                            CacheDrawnFrameBuffer = true,
                            // The 'hole' is achieved by subtracting the result of this container with the parent
                            Blending = new BlendingParameters { AlphaEquation = BlendingEquation.ReverseSubtract },
                            Child = subtractionLayer = new CircularContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                // Height computed in Update
                                Width = 1,
                                Masking = true,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        }
                    }
                }
            };

            if (drawableObject != null)
            {
                var holdNote = (DrawableHoldNote)drawableObject;

                AccentColour.BindTo(drawableObject.AccentColour);
                isHitting.BindTo(holdNote.IsHitting);
            }

            AccentColour.BindValueChanged(onAccentChanged, true);
            isHitting.BindValueChanged(_ => onAccentChanged(new ValueChangedEvent<Color4>(AccentColour.Value, AccentColour.Value)), true);
        }

        private void onAccentChanged(ValueChangedEvent<Color4> accent)
        {
            Foreground.Colour = accent.NewValue.Opacity(0.5f);
            Background.Colour = accent.NewValue.Opacity(0.7f);

            const float animation_length = 50;

            Foreground.ClearTransforms(false, nameof(Foreground.Colour));

            if (isHitting.Value)
            {
                // wait for the next sync point
                double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);
                using (Foreground.BeginDelayedSequence(synchronisedOffset))
                    Foreground.FadeColour(accent.NewValue.Lighten(0.2f), animation_length).Then().FadeColour(Foreground.Colour, animation_length).Loop();
            }

            subtractionCache.Invalidate();
        }

        protected override void Update()
        {
            base.Update();

            if (!subtractionCache.IsValid)
            {
                subtractionLayer.Width = 5;
                subtractionLayer.Height = Math.Max(0, DrawHeight - DrawWidth);
                subtractionLayer.EdgeEffect = new EdgeEffectParameters
                {
                    Colour = Color4.White,
                    Type = EdgeEffectType.Glow,
                    Radius = DrawWidth
                };

                Foreground.ForceRedraw();
                subtractionContainer.ForceRedraw();

                subtractionCache.Validate();
            }
        }
    }
}
