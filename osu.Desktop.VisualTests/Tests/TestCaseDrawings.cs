using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens.Testing;
using osu.Game.Screens.Tournament;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseDrawings : TestCase
    {
        public override string Name => @"Drawings";
        public override string Description => "Tournament drawings";

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            string[] testTeams =
            {
                "GB:United Kingdom:UK",
                "FR:France:FRA",
                "CN:China:CHN",
                "AU:Australia:AUS",
                "JP:Japan:JPN",
                "RO:Romania",
                "IT:Italy",
                "VE:Venezuela:VNZ"
            };

            using (Stream stream = storage.GetStream(Drawings.TEAMS_FILENAME, FileAccess.Write, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                foreach (string line in testTeams)
                    sw.WriteLine(line);
            }
        }

        public override void Reset()
        {
            base.Reset();

            Add(new Drawings());
        }
    }
}
