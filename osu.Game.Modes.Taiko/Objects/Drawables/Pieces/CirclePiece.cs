// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// The body of this piece will overshoot its parent by <see cref="CirclePiece.Height"/> to form 
    /// a rounded (_[-Width-]_) figure such that a regular "circle" is the result of a parent with Width = 0.
    /// </para>
    /// </summary>
    public class CirclePiece : Container, IHasAccentColour
    {
        public const float SYMBOL_SIZE = TaikoHitObject.CIRCLE_RADIUS * 2f * 0.45f;
        public const float SYMBOL_BORDER = 8;
        public const float SYMBOL_INNER_SIZE = SYMBOL_SIZE - 2 * SYMBOL_BORDER;

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

                background.Colour = AccentColour;

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

        protected override Container<Drawable> Content => SymbolContainer;
        protected readonly Container SymbolContainer;

        private readonly Container background;
        private readonly Container innerLayer;

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
                Children = new Drawable[]
                {
                    background = new CircularContainer
                    {
                        Name = "Background",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            },
                            new Triangles
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                ColourLight = Color4.White,
                                ColourDark = Color4.White.Darken(0.1f)
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
            background.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour,
                Radius = KiaiMode ? 50 : 8
            };
        }
    }
}