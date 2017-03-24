// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using System;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// The body of this piece will overshoot it by Height/2 on both sides of its length, such that
    /// a regular "circle" is the result of setting Width to 0.
    /// </para>
    /// <para>
    /// Hitobjects that have a length (e.g. DrumRolls) need only to set Width and the extra corner radius will be added internally.
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

        public override Vector2 Size => new Vector2(base.Size.X, TaikoHitObject.CIRCLE_RADIUS * 2);

        private readonly Container innerLayer;
        private readonly Container innerCircleContainer;
        private readonly Box innerBackground;
        private readonly Triangles triangles;
        private readonly Sprite symbol;

        private readonly string symbolName;

        public CirclePiece(string symbolName)
        {
            this.symbolName = symbolName;

            // The "inner layer" overshoots the the CirclePiece by Height/2 px on both sides
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
                    symbol = new Sprite
                    {
                        Name = "Symbol",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (!string.IsNullOrEmpty(symbolName))
                symbol.Texture = textures.Get($@"Play/Taiko/{symbolName}-symbol");
        }

        protected override void Update()
        {
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