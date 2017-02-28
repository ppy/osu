using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;

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

            Add(new BackgroundScreenCustom(@"Backgrounds/bg1"));

            Add(new[]
            {
                new TaikoPlayField2()
            });
        }

        class TaikoPlayField2 : Container
        {
            private Container notesContainer;

            public TaikoPlayField2()
            {
                RelativeSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    // Base field
                    new FlowContainer()
                    {
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(1, 106),

                        Children = new Drawable[]
                        {
                            // Left area
                            new Container()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.25f, 1),

                                Masking = true,

                                BorderColour = Color4.Black,
                                BorderThickness = 1,

                                Depth = 2f,

                                Children = new Drawable[]
                                {
                                    // Background
                                    new Box()
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = new Color4(17, 17, 17, 255)
                                    },
                                    new TaikoInputDrum()
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,

                                        RelativePositionAxes = Axes.X,
                                        Position = new Vector2(0.10f, 0)
                                    }
                                },
                            },
                            // Right area
                            new Container()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.75f, 1),

                                Masking = true,

                                Children = new Drawable[]
                                {
                                    // Background
                                    new Box()
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = new Color4(0, 0, 0, 127)
                                    },
                                    // Hit area + notes
                                    new Container()
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RelativePositionAxes = Axes.Both,

                                        Position = new Vector2(0.1f, 0),

                                        Children = new Drawable[]
                                        {
                                            new TaikoHitTarget()
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.Centre
                                            },
                                            // Todo: Add notes here
                                            notesContainer = new Container()
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                            }
                                        }
                                    },
                                    // Barlines
                                    new Container()
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    // Notes
                                    new Container()
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    }
                                },

                                BorderColour = new Color4(17, 17, 17, 255),
                                BorderThickness = 2
                            }
                        }
                    },
                };
            }
        }

        class TaikoInputDrum : Container
        {
            public TaikoInputDrum()
            {
                Size = new Vector2(86);

                Children = new Drawable[]
                {
                    new TaikoHalfDrum(false)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,

                        RelativeSizeAxes = Axes.Both,

                        Keys = new List<Key>(new[] { Key.F, Key.D })
                    },
                    new TaikoHalfDrum(true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,

                        RelativeSizeAxes = Axes.Both,

                        Position = new Vector2(-1f, 0),

                        Keys = new List<Key>(new[] { Key.J, Key.K })
                    }
                };
            }

            class TaikoHalfDrum : Container
            {
                /// <summary>
                /// Keys[0] -> Inner key
                /// Keys[0] -> Outer key
                /// </summary>
                public List<Key> Keys = new List<Key>();

                private Sprite outer;
                private Sprite outerHit;
                private Sprite inner;
                private Sprite innerHit;

                public TaikoHalfDrum(bool flipped)
                {
                    Masking = true;

                    Children = new Drawable[]
                    {
                        outer = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both
                        },
                        outerHit = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Colour = new Color4(102, 204, 255, 255),
                            Alpha = 0,

                            BlendingMode = BlendingMode.Additive
                        },
                        inner = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f)
                        },
                        innerHit = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),

                            Colour = new Color4(255, 102, 194, 255),
                            Alpha = 0,

                            BlendingMode = BlendingMode.Additive
                        }
                    };
                }

                [BackgroundDependencyLoader]
                private void load(TextureStore textures)
                {
                    outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
                    outerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer-hit");
                    inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
                    innerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner-hit");
                }

                protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
                {
                    if (args.Repeat)
                        return false;

                    if (args.Key == Keys[0])
                    {
                        innerHit.FadeIn();
                        innerHit.Delay(100).FadeOut(100);
                    }

                    if (args.Key == Keys[1])
                    {
                        outerHit.FadeIn();
                        outerHit.Delay(100).FadeOut(100);
                    }

                    return false;
                }
            }
        }

        class TaikoHitTarget : Container
        {
            private Sprite outer;
            private Sprite inner;

            public TaikoHitTarget()
            {
                Size = new Vector2(106);

                Children = new Drawable[]
                {
                    new Box()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Y,
                        Size = new Vector2(5, 1),

                        Colour = Color4.Black
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.7f),

                        Children = new[]
                        {
                            outer = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,

                                RelativeSizeAxes = Axes.Both,
                            },
                            inner = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,

                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.7f)
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
                inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
            }
        }

        [Flags]
        enum HitCircleStyle
        {
            Don,
            Katsu,
            Finisher
        }
    }
}
