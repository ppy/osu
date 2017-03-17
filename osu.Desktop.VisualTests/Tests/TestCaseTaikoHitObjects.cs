using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoHitObjects : TestCase
    {
        public override string Description => "Taiko hit objects";

        public override void Reset()
        {
            base.Reset();

            Add(new CentreHitCirclePiece
            {
                Position = new Vector2(100, 100),
                AccentColour = Color4.Red
            });

            Add(new RimHitCirclePiece
            {
                Position = new Vector2(100, 250),
                AccentColour = Color4.Blue
            });

            Add(new BashCirclePiece
            {
                Position = new Vector2(100, 400),
                AccentColour = Color4.Orange
            });

            Add(new DrumRollCirclePiece
            {
                Width = 256,
                Position = new Vector2(100, 550),
                AccentColour = Color4.Yellow
            });
        }
    }

    class FinisherCirclePiece : Container
    {
        public FinisherCirclePiece()
        {
            Anchor = Anchor.CentreLeft;

        }
    }

    /// <summary>
    /// A circle piece which is used to visualise RimHit objects.
    /// </summary>
    class RimHitCirclePiece : CirclePiece
    {
        public RimHitCirclePiece()
        {
            Height = 128;
        }

        protected override Drawable CreateIcon()
        {
            return new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(61f),

                BorderThickness = 8,
                BorderColour = Color4.White,

                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            };
        }
    }

    /// <summary>
    /// A circle piece which is used to visualise CentreHit objects.
    /// </summary>
    class CentreHitCirclePiece : CirclePiece
    {
        public CentreHitCirclePiece()
        {
            Height = 128;
        }

        protected override Drawable CreateIcon()
        {
            return new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(45f),

                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 1
                    }
                }
            };
        }
    }

    /// <summary>
    /// A circle piece which is used to visualise Bash objects.
    /// </summary>
    class BashCirclePiece : CirclePiece
    {
        public BashCirclePiece()
        {
            Height = 128;
        }

        protected override Drawable CreateIcon()
        {
            return new TextAwesome
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                TextSize = 45f,
                Icon = FontAwesome.fa_asterisk
            };
        }
    }

    /// <summary>
    /// A circle piece which is used to visualise DrumRoll objects.
    /// </summary>
    class DrumRollCirclePiece : CirclePiece
    {
        public DrumRollCirclePiece()
        {
            Height = 128;
        }
    }

    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// This is used uniformly throughout all osu!taiko hitobjects.
    /// <para>
    /// The contents of this piece will overshoot it by 64px on both sides on the X-axis, such that
    /// a regular "circle" is created by setting the width of this piece to 0px (resulting in a 64px radius circle).
    /// </para>
    /// </summary>
    abstract class CirclePiece : Container
    {
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

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public Color4 AccentColour;

        private Container innerLayer;
        private Container backingGlowContainer;
        private Container innerCircleContainer;

        public CirclePiece()
        {
            Container iconContainer;

            Origin = Anchor.CentreLeft;

            Children = new Drawable[]
            {
                // The "inner layer" overshoots the ObjectPiece by 64px on both sides
                innerLayer = new Container
                {
                    Name = "Inner Layer",

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Y,

                    Children = new Drawable[]
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

                                    Alpha = 0.75f
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

            Drawable icon = CreateIcon();

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

            innerCircleContainer.Colour = AccentColour;
        }

        protected override void Update()
        {
            innerLayer.Width = DrawWidth + 128;
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
        protected virtual Drawable CreateIcon() => null;
    }
}
