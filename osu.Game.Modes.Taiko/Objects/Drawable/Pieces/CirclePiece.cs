// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using OpenTK.Graphics;
using System;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// The body of this piece will overshoot its parent by <see cref="CirclePiece.Height"/> to form 
    /// a rounded (_[-Width-]_) figure such that a regular "circle" is the result of a parent with Width = 0.
    /// </para>
    /// </summary>
    public class CirclePiece : Container
    {
        private Color4 accentColour;
        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;

                innerBackground.Colour = AccentColour;

                triangles.ColourLight = AccentColour;
                triangles.ColourDark = AccentColour.Darken(0.1f);

                resetEdgeEffects();
            }
        }

        private bool kiaiMode;
        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public bool KiaiMode
        {
            get { return kiaiMode; }
            set
            {
                kiaiMode = value;

                resetEdgeEffects();
            }
        }

        public override Anchor Origin
        {
            get { return Anchor.CentreLeft; }
            set { throw new InvalidOperationException($"{nameof(CirclePiece)} must always use CentreLeft origin."); }
        }

        protected override Container<Framework.Graphics.Drawable> Content => SymbolContainer;
        protected readonly Container SymbolContainer;

        private readonly Container innerLayer;
        private readonly Container innerCircleContainer;
        private readonly Box innerBackground;
        private readonly Triangles triangles;

        public CirclePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = TaikoHitObject.CIRCLE_RADIUS * 2;

            // The "inner layer" is the body of the CirclePiece that overshoots it by Height/2 px on both sides
            AddInternal(innerLayer = new Container
            {
                Name = "Inner Layer",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Children = new Framework.Graphics.Drawable[]
                {
                    innerCircleContainer = new CircularContainer
                    {
                        Name = "Inner Circle",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            innerBackground = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            },
                            triangles = new Triangles
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    },
                    new CircularContainer
                    {
                        Name = "Ring",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 8,
                        BorderColour = Color4.White,
                        Masking = true,
                        Children = new[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    },
                    SymbolContainer = new Container
                    {
                        Name = "Symbol",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            });
        }

        protected override void Update()
        {
            // Add the overshoot to compensate for corner radius
            innerLayer.Width = DrawWidth + DrawHeight;
        }

        private void resetEdgeEffects()
        {
            innerCircleContainer.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour,
                Radius = KiaiMode ? 50 : 8
            };
        }
    }
}