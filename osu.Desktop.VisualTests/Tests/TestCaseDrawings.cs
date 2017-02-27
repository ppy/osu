using osu.Framework.Screens.Testing;
using osu.Game.Screens.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseDrawings : TestCase
    {
        public override string Name => @"Drawings";
        public override string Description => "Tournament drawings";

        public override void Reset()
        {
            base.Reset();

            Add(new Drawings());
        }
    }
}
