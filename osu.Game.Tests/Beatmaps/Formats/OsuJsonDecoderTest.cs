// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class OsuJsonDecoderTest
    {
        private const string normal = "Soleily - Renatus (Gamu) [Insane].osu";
        private const string marathon = "Within Temptation - The Unforgiving (Armin) [Marathon].osu";
        private const string with_sb = "Kozato snow - Rengetsu Ouka (_Kiva) [Yuki YukI].osu";

        [Test]
        public void TestDecodeMetadata()
        {
            var beatmap = decodeAsJson(normal);
            var meta = beatmap.BeatmapInfo.Metadata;
            ClassicAssert.AreEqual(241526, beatmap.BeatmapInfo.BeatmapSet?.OnlineID);
            ClassicAssert.AreEqual("Soleily", meta.Artist);
            ClassicAssert.AreEqual("Soleily", meta.ArtistUnicode);
            ClassicAssert.AreEqual("03. Renatus - Soleily 192kbps.mp3", meta.AudioFile);
            ClassicAssert.AreEqual("Gamu", meta.Author.Username);
            ClassicAssert.AreEqual("machinetop_background.jpg", meta.BackgroundFile);
            ClassicAssert.AreEqual(164471, meta.PreviewTime);
            ClassicAssert.AreEqual(string.Empty, meta.Source);
            ClassicAssert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", meta.Tags);
            ClassicAssert.AreEqual("Renatus", meta.Title);
            ClassicAssert.AreEqual("Renatus", meta.TitleUnicode);
        }

        [Test]
        public void TestDecodeGeneral()
        {
            var beatmap = decodeAsJson(normal);
            var beatmapInfo = beatmap.BeatmapInfo;
            ClassicAssert.AreEqual(0, beatmap.AudioLeadIn);
            ClassicAssert.AreEqual(0.7f, beatmap.StackLeniency);
            ClassicAssert.AreEqual(false, beatmap.SpecialStyle);
            ClassicAssert.True(beatmapInfo.Ruleset.OnlineID == 0);
            ClassicAssert.AreEqual(false, beatmap.LetterboxInBreaks);
            ClassicAssert.AreEqual(false, beatmap.WidescreenStoryboard);
            ClassicAssert.AreEqual(CountdownType.None, beatmap.Countdown);
            ClassicAssert.AreEqual(0, beatmap.CountdownOffset);
        }

        [Test]
        public void TestDecodeEditor()
        {
            var beatmap = decodeAsJson(normal);
            var beatmapInfo = beatmap.BeatmapInfo;

            int[] expectedBookmarks =
            {
                11505, 22054, 32604, 43153, 53703, 64252, 74802, 85351,
                95901, 106450, 116999, 119637, 130186, 140735, 151285,
                161834, 164471, 175020, 185570, 196119, 206669, 209306
            };
            ClassicAssert.AreEqual(expectedBookmarks.Length, beatmap.Bookmarks.Length);
            for (int i = 0; i < expectedBookmarks.Length; i++)
                ClassicAssert.AreEqual(expectedBookmarks[i], beatmap.Bookmarks[i]);
            ClassicAssert.AreEqual(1.8, beatmap.DistanceSpacing);
            ClassicAssert.AreEqual(4, beatmapInfo.BeatDivisor);
            ClassicAssert.AreEqual(4, beatmap.GridSize);
            ClassicAssert.AreEqual(2, beatmap.TimelineZoom);
        }

        [Test]
        public void TestDecodeDifficulty()
        {
            var beatmap = decodeAsJson(normal);
            var difficulty = beatmap.Difficulty;
            ClassicAssert.AreEqual(6.5f, difficulty.DrainRate);
            ClassicAssert.AreEqual(4, difficulty.CircleSize);
            ClassicAssert.AreEqual(8, difficulty.OverallDifficulty);
            ClassicAssert.AreEqual(9, difficulty.ApproachRate);
            ClassicAssert.AreEqual(1.8, difficulty.SliderMultiplier);
            ClassicAssert.AreEqual(2, difficulty.SliderTickRate);
        }

        [Test]
        public void TestDecodePostConverted()
        {
            var converted = new OsuBeatmapConverter(decodeAsJson(normal), new OsuRuleset()).Convert();

            var processor = new OsuBeatmapProcessor(converted);

            processor.PreProcess();
            foreach (var o in converted.HitObjects)
                o.ApplyDefaults(converted.ControlPointInfo, converted.Difficulty);
            processor.PostProcess();

            var beatmap = converted.Serialize().Deserialize<Beatmap>();

            var curveData = beatmap.HitObjects[0] as IHasPathWithRepeats;
            var positionData = beatmap.HitObjects[0] as IHasPosition;

            Assert.That(positionData, Is.Not.Null);
            Assert.That(curveData, Is.Not.Null);
            ClassicAssert.AreEqual(90, curveData.Path.Distance);
            ClassicAssert.AreEqual(new Vector2(192, 168), positionData.Position);
            ClassicAssert.AreEqual(956, beatmap.HitObjects[0].StartTime);
            ClassicAssert.True(beatmap.HitObjects[0].Samples.Any(s => s.Name == HitSampleInfo.HIT_NORMAL));

            positionData = beatmap.HitObjects[1] as IHasPosition;

            Assert.That(positionData, Is.Not.Null);
            ClassicAssert.AreEqual(new Vector2(304, 56), positionData.Position);
            ClassicAssert.AreEqual(1285, beatmap.HitObjects[1].StartTime);
            ClassicAssert.True(beatmap.HitObjects[1].Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP));
        }

        [Test]
        public void TestDecodeHitObjects()
        {
            var beatmap = decodeAsJson(normal);

            var curveData = beatmap.HitObjects[0] as IHasPathWithRepeats;
            var positionData = beatmap.HitObjects[0] as IHasPosition;

            Assert.That(positionData, Is.Not.Null);
            Assert.That(curveData, Is.Not.Null);
            ClassicAssert.AreEqual(90, curveData.Path.Distance);
            ClassicAssert.AreEqual(new Vector2(192, 168), positionData.Position);
            ClassicAssert.AreEqual(956, beatmap.HitObjects[0].StartTime);
            ClassicAssert.True(beatmap.HitObjects[0].Samples.Any(s => s.Name == HitSampleInfo.HIT_NORMAL));

            positionData = beatmap.HitObjects[1] as IHasPosition;

            Assert.That(positionData, Is.Not.Null);
            ClassicAssert.AreEqual(new Vector2(304, 56), positionData.Position);
            ClassicAssert.AreEqual(1285, beatmap.HitObjects[1].StartTime);
            ClassicAssert.True(beatmap.HitObjects[1].Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP));
        }

        // [TestCase(normal)]
        // [TestCase(marathon)]
        // [Ignore("temporarily disabled pending DeepEqual fix (https://github.com/jamesfoster/DeepEqual/pull/35)")]
        // // Currently fails:
        // // [TestCase(with_sb)]
        // public void TestParity(string beatmap)
        // {
        //     var legacy = decode(beatmap, out Beatmap json);
        //     json.WithDeepEqual(legacy)
        //         .IgnoreProperty(r => r.DeclaringType == typeof(HitWindows)
        //                              // Todo: CustomSampleBank shouldn't exist going forward, we need a conversion mechanism
        //                              || r.Name == nameof(LegacyDecoder<Beatmap>.LegacySampleControlPoint.CustomSampleBank))
        //         .Assert();
        // }

        [Test]
        public void TestGetJsonDecoder()
        {
            Decoder<Beatmap> decoder;

            using (var stream = TestResources.OpenResource(normal))
            using (var sr = new LineBufferedReader(stream))
            {
                var legacyDecoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr);

                using (var memStream = new MemoryStream())
                using (var memWriter = new StreamWriter(memStream))
                using (var memReader = new LineBufferedReader(memStream))
                {
                    memWriter.Write(legacyDecoded.Serialize());
                    memWriter.Flush();

                    memStream.Position = 0;
                    decoder = Decoder.GetDecoder<Beatmap>(memReader);
                }
            }

            ClassicAssert.IsInstanceOf(typeof(JsonBeatmapDecoder), decoder);
        }

        /// <summary>
        /// Reads a .osu file first with a <see cref="LegacyBeatmapDecoder"/>, serializes the resulting <see cref="Beatmap"/> to JSON
        /// and then deserializes the result back into a <see cref="Beatmap"/> through an <see cref="JsonBeatmapDecoder"/>.
        /// </summary>
        /// <param name="filename">The .osu file to decode.</param>
        /// <returns>The <see cref="Beatmap"/> after being decoded by an <see cref="LegacyBeatmapDecoder"/>.</returns>
        private Beatmap decodeAsJson(string filename)
        {
            decode(filename, out Beatmap jsonDecoded);
            return jsonDecoded;
        }

        /// <summary>
        /// Reads a .osu file first with a <see cref="LegacyBeatmapDecoder"/>, serializes the resulting <see cref="Beatmap"/> to JSON
        /// and then deserializes the result back into a <see cref="Beatmap"/> through an <see cref="JsonBeatmapDecoder"/>.
        /// </summary>
        /// <param name="filename">The .osu file to decode.</param>
        /// <param name="jsonDecoded">The <see cref="Beatmap"/> after being decoded by an <see cref="JsonBeatmapDecoder"/>.</param>
        /// <returns>The <see cref="Beatmap"/> after being decoded by an <see cref="LegacyBeatmapDecoder"/>.</returns>
        private Beatmap decode(string filename, out Beatmap jsonDecoded)
        {
            using (var stream = TestResources.OpenResource(filename))
            using (var sr = new LineBufferedReader(stream))
            {
                var legacyDecoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr);

                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var sr2 = new LineBufferedReader(ms))
                {
                    sw.Write(legacyDecoded.Serialize());
                    sw.Flush();

                    ms.Position = 0;

                    jsonDecoded = new JsonBeatmapDecoder().Decode(sr2);
                    return legacyDecoded;
                }
            }
        }
    }
}
