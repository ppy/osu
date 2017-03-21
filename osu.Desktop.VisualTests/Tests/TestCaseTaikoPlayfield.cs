// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens.Testing;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoPlayfield : TestCase
    {
        public override string Description => "Taiko playfield";

        public override void Reset()
        {
            base.Reset();

            Add(new TaikoPlayfield
            {
                Y = 200
            });
        }
    }
}
