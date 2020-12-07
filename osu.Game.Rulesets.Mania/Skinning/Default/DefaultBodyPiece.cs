// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    public class DefaultBodyPiece : CompositeDrawable, IHoldNoteBody
    {
        protected readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
        protected readonly IBindable<bool> IsHitting = new Bindable<bool>();

        protected Drawable Background { get; private set; }
        private Container foregroundContainer;

        public DefaultBodyPiece()
        {
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] DrawableHitObject drawableObject)
        {
            InternalChildren = new[]
            {
                Background = new Box { RelativeSizeAxes = Axes.Both },
                foregroundContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            if (drawableObject != null)
            {
                var holdNote = (DrawableHoldNote)drawableObject;

                AccentColour.BindTo(drawableObject.AccentColour);
                IsHitting.BindTo(holdNote.IsHitting);
            }

            AccentColour.BindValueChanged(onAccentChanged, true);

            Recycle();
        }

        public void Recycle() => foregroundContainer.Child = CreateForeground();

        protected virtual Drawable CreateForeground() => new ForegroundPiece
        {
            AccentColour = { BindTarget = AccentColour },
            IsHitting = { BindTarget = IsHitting }
        };

        private void onAccentChanged(ValueChangedEvent<Color4> accent) => Background.Colour = accent.NewValue.Opacity(0.7f);

        private class ForegroundPiece : CompositeDrawable
        {
            public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
            public readonly IBindable<bool> IsHitting = new Bindable<bool>();

            private readonly LayoutValue subtractionCache = new LayoutValue(Invalidation.DrawSize);

            private BufferedContainer foregroundBuffer;
            private BufferedContainer subtractionBuffer;
            private Container subtractionLayer;

            public ForegroundPiece()
            {
                RelativeSizeAxes = Axes.Both;

                AddLayout(subtractionCache);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = foregroundBuffer = new BufferedContainer
                {
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both },
                        subtractionBuffer = new BufferedContainer
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
                };

                AccentColour.BindValueChanged(onAccentChanged, true);
                IsHitting.BindValueChanged(_ => onAccentChanged(new ValueChangedEvent<Color4>(AccentColour.Value, AccentColour.Value)), true);
            }

            private void onAccentChanged(ValueChangedEvent<Color4> accent)
            {
                foregroundBuffer.Colour = accent.NewValue.Opacity(0.5f);

                const float animation_length = 50;

                foregroundBuffer.ClearTransforms(false, nameof(foregroundBuffer.Colour));

                if (IsHitting.Value)
                {
                    // wait for the next sync point
                    double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);
                    using (foregroundBuffer.BeginDelayedSequence(synchronisedOffset))
                        foregroundBuffer.FadeColour(accent.NewValue.Lighten(0.2f), animation_length).Then().FadeColour(foregroundBuffer.Colour, animation_length).Loop();
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

                    foregroundBuffer.ForceRedraw();
                    subtractionBuffer.ForceRedraw();

                    subtractionCache.Validate();
                }
            }
        }
    }
}
