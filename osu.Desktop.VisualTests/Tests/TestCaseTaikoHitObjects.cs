// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
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

            Add(new CentreHitCircle(new CirclePiece(@"centre")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 100)
            });

            Add(new CentreHitCircle(new AccentedCirclePiece(@"centre")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 100)
            });

            Add(new RimHitCircle(new CirclePiece(@"rim")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 300)
            });

            Add(new RimHitCircle(new AccentedCirclePiece(@"rim")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 300)
            });

            Add(new SwellCircle(new CirclePiece(@"swell")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 500)
            });

            Add(new SwellCircle(new AccentedCirclePiece(@"swell")
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 500)
            });

            Add(new DrumRollCircle(new CirclePiece(string.Empty)
            {
                Width = 250,
                KiaiMode = kiai
            })
            {
                Position = new Vector2(575, 100)
            });

            Add(new DrumRollCircle(new AccentedCirclePiece(string.Empty)
            {
                Width = 250,
                KiaiMode = kiai
            })
            {
                Position = new Vector2(575, 300)
            });
        }

        private class SwellCircle : BaseCircle
        {
            public SwellCircle(CirclePiece piece)
                : base(piece)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
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
            public CentreHitCircle(CirclePiece piece)
                : base(piece)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Piece.AccentColour = colours.PinkDarker;
            }
        }

        private class RimHitCircle : BaseCircle
        {
            public RimHitCircle(CirclePiece piece)
                : base(piece)
            {
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
