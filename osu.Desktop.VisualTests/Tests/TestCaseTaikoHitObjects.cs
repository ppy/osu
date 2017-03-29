// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
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

            Add(new CirclePiece
            {
                Position = new Vector2(100, 100),
                Width = 0,
                AccentColour = Color4.DarkRed,
                KiaiMode = kiai,
                Children = new[]
                {
                    new CentreHitSymbolPiece()
                }
            });

            Add(new StrongCirclePiece
            {
                Position = new Vector2(350, 100),
                Width = 0,
                AccentColour = Color4.DarkRed,
                KiaiMode = kiai,
                Children = new[]
                {
                    new CentreHitSymbolPiece()
                }
            });

            Add(new CirclePiece
            {
                Position = new Vector2(100, 300),
                Width = 0,
                AccentColour = Color4.DarkBlue,
                KiaiMode = kiai,
                Children = new[]
                {
                    new RimHitSymbolPiece()
                }
            });

            Add(new StrongCirclePiece
            {
                Position = new Vector2(350, 300),
                Width = 0,
                AccentColour = Color4.DarkBlue,
                KiaiMode = kiai,
                Children = new[]
                {
                    new RimHitSymbolPiece()
                }
            });

            Add(new SwellCircle(new CirclePiece
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(100, 500)
            });

            Add(new SwellCircle(new StrongCirclePiece
            {
                KiaiMode = kiai
            })
            {
                Position = new Vector2(350, 500)
            });

            Add(new DrumRollCircle(new CirclePiece
            {
                KiaiMode = kiai
            })
            {
                Width = 250,
                Position = new Vector2(575, 100)
            });

            Add(new DrumRollCircle(new StrongCirclePiece
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
            public SwellCircle(CirclePiece piece)
                : base(piece)
            {
                Piece.Add(new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = CirclePiece.SYMBOL_INNER_SIZE,
                    Icon = FontAwesome.fa_asterisk,
                    Shadow = false
                });
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
