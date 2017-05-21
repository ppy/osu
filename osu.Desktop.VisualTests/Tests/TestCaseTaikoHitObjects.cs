// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

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
                AccentColour = Color4.DarkRed,
                KiaiMode = kiai,
                Children = new[]
                {
                    new CentreHitSymbolPiece()
                }
            });

            Add(new CirclePiece(true)
            {
                Position = new Vector2(350, 100),
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
                AccentColour = Color4.DarkBlue,
                KiaiMode = kiai,
                Children = new[]
                {
                    new RimHitSymbolPiece()
                }
            });

            Add(new CirclePiece(true)
            {
                Position = new Vector2(350, 300),
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
                AccentColour = Color4.Orange,
                KiaiMode = kiai,
                Children = new[]
                {
                    new SwellSymbolPiece()
                }
            });

            Add(new ElongatedCirclePiece
            {
                Position = new Vector2(575, 100),
                AccentColour = Color4.Orange,
                KiaiMode = kiai,
                Length = 0.10f,
                PlayfieldLengthReference = () => DrawSize.X
            });

            Add(new ElongatedCirclePiece(true)
            {
                Position = new Vector2(575, 300),
                AccentColour = Color4.Orange,
                KiaiMode = kiai,
                Length = 0.10f,
                PlayfieldLengthReference = () => DrawSize.X
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
