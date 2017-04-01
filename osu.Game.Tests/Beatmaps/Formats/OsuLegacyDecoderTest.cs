﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes;
using osu.Game.Tests.Resources;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Objects.Legacy;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class OsuLegacyDecoderTest
    {
        [OneTimeSetUpAttribute]
        public void SetUp()
        {
            OsuLegacyDecoder.Register();
            Ruleset.Register(new OsuRuleset());
        }

        [Test]
        public void TestDecodeMetadata()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                var beatmap = decoder.Decode(new StreamReader(stream));
                var meta = beatmap.BeatmapInfo.Metadata;
                Assert.AreEqual(241526, meta.OnlineBeatmapSetID);
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
                var beatmapInfo = decoder.Decode(new StreamReader(stream)).BeatmapInfo;
                Assert.AreEqual(0, beatmapInfo.AudioLeadIn);
                Assert.AreEqual(false, beatmapInfo.Countdown);
                Assert.AreEqual(SampleSet.Soft, beatmapInfo.SampleSet);
                Assert.AreEqual(0.7f, beatmapInfo.StackLeniency);
                Assert.AreEqual(false, beatmapInfo.SpecialStyle);
                Assert.AreEqual(PlayMode.Osu, beatmapInfo.Mode);
                Assert.AreEqual(false, beatmapInfo.LetterboxInBreaks);
                Assert.AreEqual(false, beatmapInfo.WidescreenStoryboard);
            }
        }

        [Test]
        public void TestDecodeEditor()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                var beatmap = decoder.Decode(new StreamReader(stream)).BeatmapInfo;
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
                var beatmap = decoder.Decode(new StreamReader(stream));
                var difficulty = beatmap.BeatmapInfo.Difficulty;
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
                var beatmap = decoder.Decode(new StreamReader(stream));
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

        [Test]
        public void TestDecodeHitObjects()
        {
            var decoder = new OsuLegacyDecoder();
            using (var stream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            {
                var beatmap = decoder.Decode(new StreamReader(stream));
                var slider = beatmap.HitObjects[0] as LegacySlider;
                Assert.IsNotNull(slider);
                Assert.AreEqual(new Vector2(192, 168), slider.Position);
                Assert.AreEqual(956, slider.StartTime);
                Assert.AreEqual(SampleType.None, slider.Sample.Type);
                var hit = beatmap.HitObjects[1] as LegacyHit;
                Assert.IsNotNull(hit);
                Assert.AreEqual(new Vector2(304, 56), hit.Position);
                Assert.AreEqual(1285, hit.StartTime);
                Assert.AreEqual(SampleType.Clap, hit.Sample.Type);
            }
        }
    }
}