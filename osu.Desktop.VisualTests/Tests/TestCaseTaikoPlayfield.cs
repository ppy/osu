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

            Add(new BackgroundScreenCustom(@"Backgrounds/bg1"));

            Add(new[]
            {
                new TaikoPlayField2()
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
        }

        class DrumSet : Container
        {
            public DrumSet()
            {
                Size = new Vector2(202);

                Children = new Drawable[]
                {

                };
            }
        }

        class HitTarget : Container
        {

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
                        Size = new Vector2(1, 212),

                        Children = new Drawable[]
                        {
                            // Left area
                            new Container()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.25f, 1),

                                Masking = true,

                                Children = new Drawable[]
                                {
                                    // Background
                                    new Box()
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = new Color4(17, 17, 17, 255)
                                    },
                                    new DrumSet()
                                    {
                                        RelativePositionAxes = Axes.Both,
                                        Position = new Vector2(0.65f, 0.5f)
                                    }
                                },

                                BorderColour = Color4.Black,
                                BorderThickness = 1,

                                Depth = 2f
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

                                        Position = new Vector2(0.15f, 0),

                                        Children = new Drawable[]
                                        {
                                            new HitTarget()
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft
                                            },
                                            // Todo: Add notes here
                                            notesContainer = new Container()
                                            {
                                                RelativeSizeAxes = Axes.Both
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

        // Drawable hit objects
        //class DrawableTaikoHitObject : DrawableHitObject
        //{
        //    public override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Hit300 }
        //    public DrawableTaikoHitObject(TaikoHitObject hitObject)
        //        : base(hitObject)
        //    {

        //    }
        //}

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
        
        [Flags]
        enum HitCircleStyle
        {
            Don,
            Katsu,
            Finisher
        }
    }
}
