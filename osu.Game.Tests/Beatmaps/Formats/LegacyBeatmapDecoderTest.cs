// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Tests.Resources;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
using osu.Game.Skinning;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapDecoderTest
    {
        [Test]
        public void TestDecodeBeatmapGeneral()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var beatmapInfo = beatmap.BeatmapInfo;
                var metadata = beatmap.Metadata;

                Assert.AreEqual("03. Renatus - Soleily 192kbps.mp3", metadata.AudioFile);
                Assert.AreEqual(0, beatmapInfo.AudioLeadIn);
                Assert.AreEqual(164471, metadata.PreviewTime);
                Assert.IsFalse(beatmapInfo.Countdown);
                Assert.AreEqual(0.7f, beatmapInfo.StackLeniency);
                Assert.IsTrue(beatmapInfo.RulesetID == 0);
                Assert.IsFalse(beatmapInfo.LetterboxInBreaks);
                Assert.IsFalse(beatmapInfo.SpecialStyle);
                Assert.IsFalse(beatmapInfo.WidescreenStoryboard);
            }
        }

        [Test]
        public void TestDecodeBeatmapEditor()
        {
            var decoder = new LegacyBeatmapDecoder();
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var beatmapInfo = decoder.Decode(stream).BeatmapInfo;

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
        }

        [Test]
        public void TestDecodeBeatmapMetadata()
        {
            var decoder = new LegacyBeatmapDecoder();
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var beatmapInfo = beatmap.BeatmapInfo;
                var metadata = beatmap.Metadata;

                Assert.AreEqual("Renatus", metadata.Title);
                Assert.AreEqual("Renatus", metadata.TitleUnicode);
                Assert.AreEqual("Soleily", metadata.Artist);
                Assert.AreEqual("Soleily", metadata.ArtistUnicode);
                Assert.AreEqual("Gamu", metadata.AuthorString);
                Assert.AreEqual("Insane", beatmapInfo.Version);
                Assert.AreEqual(string.Empty, metadata.Source);
                Assert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", metadata.Tags);
                Assert.AreEqual(557821, beatmapInfo.OnlineBeatmapID);
                Assert.AreEqual(241526, metadata.OnlineBeatmapSetID);
            }
        }

        [Test]
        public void TestDecodeBeatmapDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder();
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var difficulty = decoder.Decode(stream).BeatmapInfo.BaseDifficulty;

                Assert.AreEqual(6.5f, difficulty.DrainRate);
                Assert.AreEqual(4, difficulty.CircleSize);
                Assert.AreEqual(8, difficulty.OverallDifficulty);
                Assert.AreEqual(9, difficulty.ApproachRate);
                Assert.AreEqual(1.8, difficulty.SliderMultiplier);
                Assert.AreEqual(2, difficulty.SliderTickRate);
            }
        }

        [Test]
        public void TestDecodeBeatmapEvents()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var metadata = beatmap.Metadata;
                var breakPoint = beatmap.Breaks[0];

                Assert.AreEqual("machinetop_background.jpg", metadata.BackgroundFile);
                Assert.AreEqual(122474, breakPoint.StartTime);
                Assert.AreEqual(140135, breakPoint.EndTime);
                Assert.IsTrue(breakPoint.HasEffect);
            }
        }

        [Test]
        public void TestDecodeBeatmapTimingPoints()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var controlPoints = beatmap.ControlPointInfo;

                Assert.AreEqual(4, controlPoints.TimingPoints.Count);
                var timingPoint = controlPoints.TimingPoints[0];
                Assert.AreEqual(956, timingPoint.Time);
                Assert.AreEqual(329.67032967033d, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignatures.SimpleQuadruple, timingPoint.TimeSignature);

                Assert.AreEqual(5, controlPoints.DifficultyPoints.Count);
                var difficultyPoint = controlPoints.DifficultyPoints[0];
                Assert.AreEqual(116999, difficultyPoint.Time);
                Assert.AreEqual(0.75000000000000189d, difficultyPoint.SpeedMultiplier);

                Assert.AreEqual(34, controlPoints.SamplePoints.Count);
                var soundPoint = controlPoints.SamplePoints[0];
                Assert.AreEqual(956, soundPoint.Time);
                Assert.AreEqual("soft", soundPoint.SampleBank);
                Assert.AreEqual(60, soundPoint.SampleVolume);

                Assert.AreEqual(8, controlPoints.EffectPoints.Count);
                var effectPoint = controlPoints.EffectPoints[0];
                Assert.AreEqual(53703, effectPoint.Time);
                Assert.IsTrue(effectPoint.KiaiMode);
                Assert.IsFalse(effectPoint.OmitFirstBarLine);
            }
        }

        [Test]
        public void TestDecodeBeatmapColors()
        {
            var decoder = new LegacySkinDecoder();
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var comboColors = decoder.Decode(stream).ComboColours;

                Color4[] expectedColors =
                {
                    new Color4(142, 199, 255, 255),
                    new Color4(255, 128, 128, 255),
                    new Color4(128, 255, 255, 255),
                    new Color4(128, 255, 128, 255),
                    new Color4(255, 187, 255, 255),
                    new Color4(255, 177, 140, 255),
                };
                Assert.AreEqual(expectedColors.Length, comboColors.Count);
                for (int i = 0; i < expectedColors.Length; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        [Test]
        public void TestDecodeBeatmapHitObjects()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };
            using (var resStream = Resource.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new StreamReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                var curveData = hitObjects[0] as IHasCurve;
                var positionData = hitObjects[0] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.IsNotNull(curveData);
                Assert.AreEqual(new Vector2(192, 168), positionData.Position);
                Assert.AreEqual(956, hitObjects[0].StartTime);
                Assert.IsTrue(hitObjects[0].Samples.Any(s => s.Name == SampleInfo.HIT_NORMAL));

                positionData = hitObjects[1] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.AreEqual(new Vector2(304, 56), positionData.Position);
                Assert.AreEqual(1285, hitObjects[1].StartTime);
                Assert.IsTrue(hitObjects[1].Samples.Any(s => s.Name == SampleInfo.HIT_CLAP));
            }
        }
    }
}
