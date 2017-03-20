// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using OpenTK.Graphics;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// The body of this piece will overshoot it by Height/2 on both sides of its length, such that
    /// a regular "circle" the result of setting Width to 0.
    /// </para>
    /// <para>
    /// Hitobjects that have a length need only to set Width and the extra corner radius will be added internally.
    /// </para>
    /// </summary>
    public abstract class CirclePiece : ScrollingCirclePiece
    {
        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public Color4 AccentColour { get; protected set; }

        private bool kiaiMode;
        /// <summary>
        /// Whether Kiai mode is active for this object.
        /// </summary>
        public bool KiaiMode
        {
            get { return kiaiMode; }
            set
            {
                kiaiMode = value;

                if (innerCircleContainer != null)
                    innerCircleContainer.EdgeEffect = value ? createKiaiEdgeEffect() : default(EdgeEffect);
            }
        }

        public override Vector2 Size => new Vector2(base.Size.X, 128);

        private Container innerLayer;
        private Container backingGlowContainer;
        private Container innerCircleContainer;
        private Box innerBackground;
        private Triangles triangles;

        protected CirclePiece()
        {
            Container iconContainer;

            Children = new Framework.Graphics.Drawable[]
            {
                // The "inner layer" overshoots the ObjectPiece by 64px on both sides
                innerLayer = new Container
                {
                    Name = "Inner Layer",

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Y,

                    Children = new Framework.Graphics.Drawable[]
                    {
                        backingGlowContainer = new CircularContainer
                        {
                            Name = "Backing Glow",

                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,

                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        },
                        innerCircleContainer = new CircularContainer
                        {
                            Name = "Inner Circle",

                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

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
                        iconContainer = new Container
                        {
                            Name = "Icon Container",

                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                },
            };

            Framework.Graphics.Drawable icon = CreateIcon();

            if (icon != null)
                iconContainer.Add(icon);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            backingGlowContainer.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour,
                Radius = 8
            };

            if (KiaiMode)
                innerCircleContainer.EdgeEffect = createKiaiEdgeEffect();

            innerBackground.Colour = AccentColour;

            triangles.ColourLight = AccentColour;
            triangles.ColourDark = AccentColour.Darken(0.1f);
        }

        protected override void Update()
        {
            innerLayer.Width = DrawWidth + DrawHeight;
        }

        private EdgeEffect createKiaiEdgeEffect()
        {
            return new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour,
                Radius = 50
            };
        }

        /// <summary>
        /// Creates the icon that's shown in the middle of this object piece.
        /// </summary>
        /// <returns>The icon.</returns>
        protected virtual Framework.Graphics.Drawable CreateIcon() => null;
    }
}