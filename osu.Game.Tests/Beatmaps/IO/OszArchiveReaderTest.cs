using System;
using System.IO;
using NUnit.Framework;
using osu.Game.Beatmaps.IO;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class OszArchiveReaderTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            OszArchiveReader.Register();
        }
    
        [Test]
        public void TestReadBeatmaps()
        {
            using (var osz = File.OpenRead(Resource.GetPath("241526 Soleily - Renatus.osz")))
            {
                var reader = new OszArchiveReader(osz);
                string[] expected =
                {
                    "Soleily - Renatus (Deif) [Platter].osu",
                    "Soleily - Renatus (Deif) [Rain].osu",
                    "Soleily - Renatus (Deif) [Salad].osu",
                    "Soleily - Renatus (ExPew) [Another].osu",
                    "Soleily - Renatus (ExPew) [Hyper].osu",
                    "Soleily - Renatus (ExPew) [Normal].osu",
                    "Soleily - Renatus (Gamu) [Hard].osu",
                    "Soleily - Renatus (Gamu) [Insane].osu",
                    "Soleily - Renatus (Gamu) [Normal].osu",
                    "Soleily - Renatus (MMzz) [Futsuu].osu",
                    "Soleily - Renatus (MMzz) [Muzukashii].osu",
                    "Soleily - Renatus (MMzz) [Oni].osu"
                };
                var maps = reader.ReadBeatmaps();
                foreach (var map in expected)
                    Assert.Contains(map, maps);
            }
        }
    }
}

