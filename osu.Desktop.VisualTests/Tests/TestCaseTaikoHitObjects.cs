using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

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
                Position = new Vector2(100, 100)
            });

            Add(new FinisherPiece(new CentreHitCirclePiece())
            {
                Position = new Vector2(350, 100)
            });

            Add(new RimHitCirclePiece
            {
                Position = new Vector2(100, 280)
            });

            Add(new FinisherPiece(new RimHitCirclePiece())
            {
                Position = new Vector2(350, 280)
            });

            Add(new BashCirclePiece
            {
                Position = new Vector2(100, 460)
            });

            Add(new FinisherPiece(new BashCirclePiece())
            {
                Position = new Vector2(350, 460)
            });

            Add(new DrumRollCirclePiece
            {
                Width = 250,
                Position = new Vector2(100, 640)
            });

            Add(new FinisherPiece(new DrumRollCirclePiece()
            {
                Width = 250
            })
            {
                Position = new Vector2(600, 640)
            });
        }
    }

    class FinisherPiece : ScrollingCirclePiece
    {
        public FinisherPiece(CirclePiece originalPiece)
        {
            Scale = new Vector2(1.5f);

            Children = new[]
            {
                originalPiece
            };
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.BlueDarker;
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.PinkDarker;
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.YellowDark;
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.YellowDark;
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
    abstract class CirclePiece : ScrollingCirclePiece
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
        public Color4 AccentColour { get; protected set; }

        private Container innerLayer;
        private Container backingGlowContainer;
        private Container innerCircleContainer;
        private Box innerBackground;
        private Triangles triangles;

        protected CirclePiece()
        {
            Container iconContainer;

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

            innerBackground.Colour = AccentColour;

            triangles.ColourLight = AccentColour;
            triangles.ColourDark = AccentColour.Darken(0.1f);
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

    class ScrollingCirclePiece : Container
    {
        public ScrollingCirclePiece()
        {
            Origin = Anchor.CentreLeft;

            // Todo: Relative X position
        }
    }
}
