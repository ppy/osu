using System;
using System.IO;
using NUnit.Framework;
using osu.Game.Beatmaps.IO;
using osu.Game.GameModes.Play;
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
            using (var osz = Resource.OpenResource("Beatmaps.241526 Soleily - Renatus.osz"))
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

        [Test]
        public void TestReadMetadata()
        {
            using (var osz = Resource.OpenResource("Beatmaps.241526 Soleily - Renatus.osz"))
            {
                var reader = new OszArchiveReader(osz);
                var meta = reader.ReadMetadata();
                Assert.AreEqual(241526, meta.BeatmapSetID);
                Assert.AreEqual("Soleily", meta.Artist);
                Assert.AreEqual("Soleily", meta.ArtistUnicode);
                Assert.AreEqual("03. Renatus - Soleily 192kbps.mp3", meta.AudioFile);
                Assert.AreEqual("Deif", meta.Author);
                Assert.AreEqual("machinetop_background.jpg", meta.BackgroundFile);
                Assert.AreEqual(164471, meta.PreviewTime);
                Assert.AreEqual(string.Empty, meta.Source);
                Assert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", meta.Tags);
                Assert.AreEqual("Renatus", meta.Title);
                Assert.AreEqual("Renatus", meta.TitleUnicode);
            }
        }
        [Test]
        public void TestReadFile()
        {
            using (var osz = Resource.OpenResource("Beatmaps.241526 Soleily - Renatus.osz"))
            {
                var reader = new OszArchiveReader(osz);
                using (var stream = new StreamReader(
                    reader.ReadFile("Soleily - Renatus (Deif) [Platter].osu")))
                {
                    Assert.AreEqual("osu file format v13", stream.ReadLine().Trim());
                }
            }
        }
    }
}

