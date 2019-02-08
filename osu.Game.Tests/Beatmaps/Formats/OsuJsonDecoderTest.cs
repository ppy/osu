﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using DeepEqual.Syntax;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
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
            Assert.AreEqual(241526, beatmap.BeatmapInfo.BeatmapSet.OnlineBeatmapSetID);
            Assert.AreEqual("Soleily", meta.Artist);
            Assert.AreEqual("Soleily", meta.ArtistUnicode);
            Assert.AreEqual("03. Renatus - Soleily 192kbps.mp3", meta.AudioFile);
            Assert.AreEqual("Gamu", meta.AuthorString);
            Assert.AreEqual("machinetop_background.jpg", meta.BackgroundFile);
            Assert.AreEqual(164471, meta.PreviewTime);
            Assert.AreEqual(string.Empty, meta.Source);
            Assert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", meta.Tags);
            Assert.AreEqual("Renatus", meta.Title);
            Assert.AreEqual("Renatus", meta.TitleUnicode);
        }

        [Test]
        public void TestDecodeGeneral()
        {
            var beatmap = decodeAsJson(normal);
            var beatmapInfo = beatmap.BeatmapInfo;
            Assert.AreEqual(0, beatmapInfo.AudioLeadIn);
            Assert.AreEqual(false, beatmapInfo.Countdown);
            Assert.AreEqual(0.7f, beatmapInfo.StackLeniency);
            Assert.AreEqual(false, beatmapInfo.SpecialStyle);
            Assert.IsTrue(beatmapInfo.RulesetID == 0);
            Assert.AreEqual(false, beatmapInfo.LetterboxInBreaks);
            Assert.AreEqual(false, beatmapInfo.WidescreenStoryboard);
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
            Assert.AreEqual(expectedBookmarks.Length, beatmapInfo.Bookmarks.Length);
            for (int i = 0; i < expectedBookmarks.Length; i++)
                Assert.AreEqual(expectedBookmarks[i], beatmapInfo.Bookmarks[i]);
            Assert.AreEqual(1.8, beatmapInfo.DistanceSpacing);
            Assert.AreEqual(4, beatmapInfo.BeatDivisor);
            Assert.AreEqual(4, beatmapInfo.GridSize);
            Assert.AreEqual(2, beatmapInfo.TimelineZoom);
        }

        [Test]
        public void TestDecodeDifficulty()
        {
            var beatmap = decodeAsJson(normal);
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            Assert.AreEqual(6.5f, difficulty.DrainRate);
            Assert.AreEqual(4, difficulty.CircleSize);
            Assert.AreEqual(8, difficulty.OverallDifficulty);
            Assert.AreEqual(9, difficulty.ApproachRate);
            Assert.AreEqual(1.8, difficulty.SliderMultiplier);
            Assert.AreEqual(2, difficulty.SliderTickRate);
        }

        [Test]
        public void TestDecodeHitObjects()
        {
            var beatmap = decodeAsJson(normal);

            var curveData = beatmap.HitObjects[0] as IHasCurve;
            var positionData = beatmap.HitObjects[0] as IHasPosition;

            Assert.IsNotNull(positionData);
            Assert.IsNotNull(curveData);
            Assert.AreEqual(new Vector2(192, 168), positionData.Position);
            Assert.AreEqual(956, beatmap.HitObjects[0].StartTime);
            Assert.IsTrue(beatmap.HitObjects[0].Samples.Any(s => s.Name == SampleInfo.HIT_NORMAL));

            positionData = beatmap.HitObjects[1] as IHasPosition;

            Assert.IsNotNull(positionData);
            Assert.AreEqual(new Vector2(304, 56), positionData.Position);
            Assert.AreEqual(1285, beatmap.HitObjects[1].StartTime);
            Assert.IsTrue(beatmap.HitObjects[1].Samples.Any(s => s.Name == SampleInfo.HIT_CLAP));
        }

        [TestCase(normal)]
        [TestCase(marathon)]
        // Currently fails:
        // [TestCase(with_sb)]
        public void TestParity(string beatmap)
        {
            var legacy = decode(beatmap, out Beatmap json);
            json.WithDeepEqual(legacy)
                .IgnoreProperty(r => r.DeclaringType == typeof(HitWindows)
                                     // Todo: CustomSampleBank shouldn't exist going forward, we need a conversion mechanism
                                     || r.Name == nameof(LegacyDecoder<Beatmap>.LegacySampleControlPoint.CustomSampleBank))
                .Assert();
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
            using (var sr = new StreamReader(stream))
            {
                var legacyDecoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr);
                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var sr2 = new StreamReader(ms))
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
