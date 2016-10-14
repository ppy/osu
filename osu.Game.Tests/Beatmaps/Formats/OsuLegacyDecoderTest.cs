using System;
using System.IO;
using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Samples;
using osu.Game.GameModes.Play;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class OsuLegacyDecoderTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            OsuLegacyDecoder.Register();
        }
        [Test]
        public void TestDecodeMetadata()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                var meta = beatmap.Metadata;
                Assert.AreEqual(241526, meta.BeatmapSetID);
                Assert.AreEqual("Soleily", meta.Artist);
                Assert.AreEqual("Soleily", meta.ArtistUnicode);
                Assert.AreEqual("03. Renatus - Soleily 192kbps.mp3", meta.AudioFile);
                Assert.AreEqual("Gamu", meta.Author);
                Assert.AreEqual("machinetop_background.jpg", meta.BackgroundFile);
                Assert.AreEqual(164471, meta.PreviewTime);
                Assert.AreEqual(string.Empty, meta.Source);
                Assert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", meta.Tags);
                Assert.AreEqual("Renatus", meta.Title);
                Assert.AreEqual("Renatus", meta.TitleUnicode);
            }
        }

        [Test]
        public void TestDecodeGeneral()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                Assert.AreEqual(0, beatmap.AudioLeadIn);
                Assert.AreEqual(false, beatmap.Countdown);
                Assert.AreEqual(SampleSet.Soft, beatmap.SampleSet);
                Assert.AreEqual(0.7f, beatmap.StackLeniency);
                Assert.AreEqual(false, beatmap.SpecialStyle);
                Assert.AreEqual(PlayMode.Osu, beatmap.Mode);
                Assert.AreEqual(false, beatmap.LetterboxInBreaks);
                Assert.AreEqual(false, beatmap.WidescreenStoryboard);
            }
        }

        [Test]
        public void TestDecodeEditor()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                int[] expectedBookmarks =
                {
                    11505, 22054, 32604, 43153, 53703, 64252, 74802, 85351,
                    95901, 106450, 116999, 119637, 130186, 140735, 151285,
                    161834, 164471, 175020, 185570, 196119, 206669, 209306
                };
                Assert.AreEqual(expectedBookmarks.Length, beatmap.Bookmarks.Length);
                for (int i = 0; i < expectedBookmarks.Length; i++)
                    Assert.AreEqual(expectedBookmarks[i], beatmap.Bookmarks[i]);
                Assert.AreEqual(1.8, beatmap.DistanceSpacing);
                Assert.AreEqual(4, beatmap.BeatDivisor);
                Assert.AreEqual(4, beatmap.GridSize);
                Assert.AreEqual(2, beatmap.TimelineZoom);
            }
        }

        [Test]
        public void TestDecodeDifficulty()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                var difficulty = beatmap.BaseDifficulty;
                Assert.AreEqual(6.5f, difficulty.DrainRate);
                Assert.AreEqual(4, difficulty.CircleSize);
                Assert.AreEqual(8, difficulty.OverallDifficulty);
                Assert.AreEqual(9, difficulty.ApproachRate);
                Assert.AreEqual(1.8f, difficulty.SliderMultiplier);
                Assert.AreEqual(2, difficulty.SliderTickRate);
            }
        }

        [Test]
        public void TestDecodeColors()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                Color4[] expected =
                {
                    new Color4(142, 199, 255, 255),
                    new Color4(255, 128, 128, 255),
                    new Color4(128, 255, 255, 255),
                    new Color4(128, 255, 128, 255),
                    new Color4(255, 187, 255, 255),
                    new Color4(255, 177, 140, 255),
                };
                Assert.AreEqual(expected.Length, beatmap.ComboColors.Count);
                for (int i = 0; i < expected.Length; i++)
                    Assert.AreEqual(expected[i], beatmap.ComboColors[i]);
            }
        }

        [Test]        public void TestDecodeHitObjects()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                Beatmap beatmap = new Beatmap();
                decoder.Decode(new StreamReader(stream), beatmap);
                var slider = beatmap.HitObjects[0] as Slider;
                Assert.IsNotNull(slider);
                Assert.AreEqual(new Vector2(192, 168), slider.Position);
                Assert.AreEqual(956, slider.StartTime);
                Assert.AreEqual(SampleType.None, slider.Sample.Type);
                var circle = beatmap.HitObjects[1] as Circle;
                Assert.IsNotNull(circle);
                Assert.AreEqual(new Vector2(304, 56), circle.Position);
                Assert.AreEqual(1285, circle.StartTime);
                Assert.AreEqual(SampleType.Clap, circle.Sample.Type);
            }
        }
    }
}