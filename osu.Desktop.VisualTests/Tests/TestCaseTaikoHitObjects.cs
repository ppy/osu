// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
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

            AddToggleStep("Kiai", b =>
            {
                kiai = !kiai;
                updateKiaiState();
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

            Add(new CirclePiece
            {
                Position = new Vector2(100, 500),
                Width = 0,
                AccentColour = Color4.Orange,
                KiaiMode = kiai,
                Children = new[]
                {
                    new SwellSymbolPiece()
                }
            });

            Add(new CirclePiece
            {
                Position = new Vector2(575, 100),
                Width = 0.25f,
                AccentColour = Color4.Orange,
                KiaiMode = kiai,
            });

            Add(new StrongCirclePiece
            {
                Position = new Vector2(575, 300),
                Width = 0.25f,
                AccentColour = Color4.Orange,
                KiaiMode = kiai
            });
        }

        private void updateKiaiState()
        {
            foreach (var c in Children.OfType<CirclePiece>())
                c.KiaiMode = kiai;
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
