// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Caching;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    public class BodyPiece : Container, IHasAccentColour
    {
        private readonly Container subtractionLayer;

        protected readonly Drawable Background;
        protected readonly BufferedContainer Foreground;
        private readonly BufferedContainer subtractionContainer;

        public BodyPiece()
        {
            Blending = BlendingParameters.Additive;

            Children = new[]
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateAccentColour();
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                updateAccentColour();
            }
        }

        public bool Hitting
        {
            get => hitting;
            set
            {
                hitting = value;
                updateAccentColour();
            }
        }

        private readonly Cached subtractionCache = new Cached();

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) > 0)
                subtractionCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
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

        private bool hitting;

        private void updateAccentColour()
        {
            if (!IsLoaded)
                return;

            Foreground.Colour = AccentColour.Opacity(0.5f);
            Background.Colour = AccentColour.Opacity(0.7f);

            const float animation_length = 50;

            Foreground.ClearTransforms(false, nameof(Foreground.Colour));

            if (hitting)
            {
                // wait for the next sync point
                double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);
                using (Foreground.BeginDelayedSequence(synchronisedOffset))
                    Foreground.FadeColour(AccentColour.Lighten(0.2f), animation_length).Then().FadeColour(Foreground.Colour, animation_length).Loop();
            }

            subtractionCache.Invalidate();
        }
    }
}
