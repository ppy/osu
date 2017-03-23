﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Screens.Testing;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

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
                Position = new Vector2(100, 300)
            });

            Add(new FinisherPiece(new RimHitCirclePiece())
            {
                Position = new Vector2(350, 300)
            });

            Add(new BashCirclePiece
            {
                Position = new Vector2(100, 500)
            });

            Add(new FinisherPiece(new BashCirclePiece())
            {
                Position = new Vector2(350, 500)
            });

            Add(new DrumRollCirclePiece
            {
                Width = 250,
                Position = new Vector2(575, 100)
            });

            Add(new FinisherPiece(new DrumRollCirclePiece()
            {
                Width = 250
            })
            {
                Position = new Vector2(575, 300)
            });
        }
    }
}
