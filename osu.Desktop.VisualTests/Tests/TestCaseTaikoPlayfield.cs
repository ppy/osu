using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Taiko.UI.Drums;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseTaikoPlayfield : TestCase
    {
        public override string Name => "Taiko Playfield";
        public override string Description => "The Taiko playfield.";

        private SpriteText comboCounter;

        public override void Reset()
        {
            base.Reset();

            Add(new BackgroundModeCustom(@"Backgrounds/bg1"));

            Add(new[]
            {
                new FlowContainer()
                {
                    Position = new Vector2(0, 100),

                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, 150),

                    Children = new Drawable[]
                    {
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.25f, 1),

                            Children = new Drawable[]
                            {
                                // Drum area container
                                new TaikoDrumArea()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                // Combo counter
                                comboCounter = new SpriteText()
                                {
                                    Origin = Anchor.Centre,

                                    RelativePositionAxes = Axes.Both,
                                    Position = new Vector2(0.75f, 0.5f),

                                    Colour = new Color4(221, 255, 255, 255),

                                    Text = "8888",
                                    Font = "Venera",
                                    TextSize = 12,
                                },
                            }
                        },
                        // Track area
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.75f, 1),

                            Children = new Drawable[]
                            {
                                new TaikoTrackArea()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new TaikoHitTarget()
                                {
                                    Size = new Vector2(150, 150)
                                },
                                new TaikoHitCircle(new Color4(17, 136, 170, 255))
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Size = new Vector2(50, 50),
                                    Position = new Vector2(50, 0),

                                },
                                new TaikoHitCircle(new Color4(187, 17, 119, 255))
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Size = new Vector2(50, 50),
                                    Position = new Vector2(100, 0)
                                }
                            }
                        },
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
        }

        class TaikoHitTarget : Container
        {
            private Sprite drumBase;

            public TaikoHitTarget()
            {
                Children = new Drawable[]
                {
                    new Box()
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,

                        RelativeSizeAxes = Axes.Y,
                        Size = new Vector2(5, 1),

                        Colour = Color4.Black,
                        Alpha = 0.5f,
                    },
                    drumBase = new Sprite()
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,

                        Scale = new Vector2(0.5f),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                drumBase.Texture = textures.Get("Play/Taiko/taiko-drum@2x");
            }
        }

        class TaikoHitCircle : CircularContainer
        {
            private Sprite overlay;

            public TaikoHitCircle(Color4 innerColour)
            {
                Children = new Drawable[]
                {
                    // Background
                    new Box()
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,
                        Colour = innerColour,
                    },
                    // Triangles
                    new Triangles()
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,

                        Colour = new Color4(20, 43, 51, 255),
                    },
                    // Overlay
                    overlay = new Sprite()
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,

                        Scale = new Vector2(1.30f),

                        BlendingMode = BlendingMode.Additive
                    }
                };

                EdgeEffect = new EdgeEffect()
                {
                    Colour = innerColour,
                    Radius = 50,
                    Type = EdgeEffectType.Glow
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                overlay.Texture = textures.Get("Play/Taiko/taiko-hitcircle-overlay@2x");
            }
        }

        class TaikoDrumArea : Container
        {
            public TaikoDrumArea()
            {
                Children = new Drawable[]
                {
                    // Background
                    new BufferedContainer()
                    {
                        RelativeSizeAxes = Axes.Both,

                        Masking = true,
                        BorderColour = Color4.Black,
                        BorderThickness = 1,

                        Children = new []
                        {
                            new Box()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = new Color4(17, 17, 17, 255),
                            },
                        }
                    },

                    // Drums
                    new TaikoDrumSet(new[] { Key.Z, Key.X, Key.V, Key.C })
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.CentreLeft,

                        RelativePositionAxes = Axes.X,

                        Position = new Vector2(0.75f, 0.0f),

                        Size = new Vector2(100, 100)
                    }
                };
            }
        }

        class TaikoTrackArea : Container
        {
            public TaikoTrackArea()
            {
                Children = new[]
                {
                    // Background
                    new BufferedContainer()
                    {
                        RelativeSizeAxes = Axes.Both,

                        Masking = true,
                        BorderThickness = 2,
                        BorderColour = new Color4(85, 85, 85, 255),

                        Children = new []
                        {
                            new Box()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = new Color4(0, 0, 0, 127)
                            }
                        }
                    }
                };
            }
        }
    }
}
