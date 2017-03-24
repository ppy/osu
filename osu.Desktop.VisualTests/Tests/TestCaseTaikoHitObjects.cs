// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoHitObjects : TestCase
    {
        public override string Description => "Taiko hit objects";

        private bool kiai;

        public override void Reset()
        {
            base.Reset();

            AddToggle("Kiai", b =>
            {
                kiai = !kiai;
                Reset();
            });

            Add(new CentreHitCircle(new CirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 100)
            });

            Add(new CentreHitCircle(new AccentedCirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 100)
            });

            Add(new RimHitCircle(new CirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 300)
            });

            Add(new RimHitCircle(new AccentedCirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 300)
            });

            Add(new SwellCircle(new CirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 500)
            });

            Add(new SwellCircle(new AccentedCirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 500)
            });

            Add(new DrumRollCircle(new CirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Width = 250,
                Position = new Vector2(575, 100)
            });

            Add(new DrumRollCircle(new AccentedCirclePiece()
            {
                KiaiMode = kiai
            })
            {
                Width = 250,
                Position = new Vector2(575, 300)
            });
        }

        private class SwellCircle : BaseCircle
        {
            private const float symbol_size = TaikoHitObject.CIRCLE_RADIUS * 2f * 0.35f;

            public SwellCircle(CirclePiece piece)
                : base(piece)
            {
                Piece.Add(new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = symbol_size,
                    Shadow = false
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, TextureStore textures)
            {
                Piece.AccentColour = colours.YellowDark;
            }
        }

        private class DrumRollCircle : BaseCircle
        {
            public DrumRollCircle(CirclePiece piece)
                : base(piece)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Piece.AccentColour = colours.YellowDark;
            }
        }

        private class CentreHitCircle : BaseCircle
        {
            private const float symbol_size = TaikoHitObject.CIRCLE_RADIUS * 2f * 0.35f;

            public CentreHitCircle(CirclePiece piece)
                : base(piece)
            {
                Piece.Add(new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(symbol_size),
                    Masking = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Piece.AccentColour = colours.PinkDarker;
            }
        }

        private class RimHitCircle : BaseCircle
        {
            private const float symbol_size = TaikoHitObject.CIRCLE_RADIUS * 2f * 0.45f;

            public RimHitCircle(CirclePiece piece)
                : base(piece)
            {
                Piece.Add(new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(symbol_size),
                    BorderThickness = 8,
                    BorderColour = Color4.White,
                    Masking = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Piece.AccentColour = colours.BlueDarker;
            }
        }

        private abstract class BaseCircle : Container
        {
            protected readonly CirclePiece Piece;

            protected BaseCircle(CirclePiece piece)
            {
                Piece = piece;

                Add(Piece);
            }
        }
    }
}
