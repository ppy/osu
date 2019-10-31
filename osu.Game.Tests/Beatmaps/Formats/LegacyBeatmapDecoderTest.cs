// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using NUnit.Framework;
using osuTK;
using osuTK.Graphics;
using osu.Game.Tests.Resources;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Skinning;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapDecoderTest
    {
        [Test]
        public void TestDecodeBeatmapVersion()
        {
            using (var resStream = TestResources.OpenResource("beatmap-version.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);
                var working = new TestWorkingBeatmap(decoder.Decode(stream));

                Assert.AreEqual(6, working.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(6, working.Beatmap.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(6, working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo, Array.Empty<Mod>()).BeatmapInfo.BeatmapVersion);
            }
        }

        [Test]
        public void TestDecodeBeatmapGeneral()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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
                Assert.AreEqual(241526, beatmapInfo.BeatmapSet.OnlineBeatmapSetID);
            }
        }

        [Test]
        public void TestDecodeBeatmapDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder();

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var controlPoints = beatmap.ControlPointInfo;

                Assert.AreEqual(4, controlPoints.TimingPoints.Count);
                Assert.AreEqual(5, controlPoints.DifficultyPoints.Count);
                Assert.AreEqual(34, controlPoints.SamplePoints.Count);
                Assert.AreEqual(8, controlPoints.EffectPoints.Count);

                var timingPoint = controlPoints.TimingPointAt(0);
                Assert.AreEqual(956, timingPoint.Time);
                Assert.AreEqual(329.67032967033, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignatures.SimpleQuadruple, timingPoint.TimeSignature);

                timingPoint = controlPoints.TimingPointAt(48428);
                Assert.AreEqual(956, timingPoint.Time);
                Assert.AreEqual(329.67032967033d, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignatures.SimpleQuadruple, timingPoint.TimeSignature);

                timingPoint = controlPoints.TimingPointAt(119637);
                Assert.AreEqual(119637, timingPoint.Time);
                Assert.AreEqual(659.340659340659, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignatures.SimpleQuadruple, timingPoint.TimeSignature);

                var difficultyPoint = controlPoints.DifficultyPointAt(0);
                Assert.AreEqual(0, difficultyPoint.Time);
                Assert.AreEqual(1.0, difficultyPoint.SpeedMultiplier);

                difficultyPoint = controlPoints.DifficultyPointAt(48428);
                Assert.AreEqual(0, difficultyPoint.Time);
                Assert.AreEqual(1.0, difficultyPoint.SpeedMultiplier);

                difficultyPoint = controlPoints.DifficultyPointAt(116999);
                Assert.AreEqual(116999, difficultyPoint.Time);
                Assert.AreEqual(0.75, difficultyPoint.SpeedMultiplier, 0.1);

                var soundPoint = controlPoints.SamplePointAt(0);
                Assert.AreEqual(956, soundPoint.Time);
                Assert.AreEqual("soft", soundPoint.SampleBank);
                Assert.AreEqual(60, soundPoint.SampleVolume);

                soundPoint = controlPoints.SamplePointAt(53373);
                Assert.AreEqual(53373, soundPoint.Time);
                Assert.AreEqual("soft", soundPoint.SampleBank);
                Assert.AreEqual(60, soundPoint.SampleVolume);

                soundPoint = controlPoints.SamplePointAt(119637);
                Assert.AreEqual(119637, soundPoint.Time);
                Assert.AreEqual("soft", soundPoint.SampleBank);
                Assert.AreEqual(80, soundPoint.SampleVolume);

                var effectPoint = controlPoints.EffectPointAt(0);
                Assert.AreEqual(0, effectPoint.Time);
                Assert.IsFalse(effectPoint.KiaiMode);
                Assert.IsFalse(effectPoint.OmitFirstBarLine);

                effectPoint = controlPoints.EffectPointAt(53703);
                Assert.AreEqual(53703, effectPoint.Time);
                Assert.IsTrue(effectPoint.KiaiMode);
                Assert.IsFalse(effectPoint.OmitFirstBarLine);

                effectPoint = controlPoints.EffectPointAt(119637);
                Assert.AreEqual(95901, effectPoint.Time);
                Assert.IsFalse(effectPoint.KiaiMode);
                Assert.IsFalse(effectPoint.OmitFirstBarLine);
            }
        }

        [Test]
        public void TestDecodeOverlappingTimingPoints()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("overlapping-control-points.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.DifficultyPointAt(500).SpeedMultiplier, Is.EqualTo(1.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(1500).SpeedMultiplier, Is.EqualTo(1.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(2500).SpeedMultiplier, Is.EqualTo(0.75).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(3500).SpeedMultiplier, Is.EqualTo(1.5).Within(0.1));

                Assert.That(controlPoints.EffectPointAt(500).KiaiMode, Is.True);
                Assert.That(controlPoints.EffectPointAt(1500).KiaiMode, Is.True);
                Assert.That(controlPoints.EffectPointAt(2500).KiaiMode, Is.False);
                Assert.That(controlPoints.EffectPointAt(3500).KiaiMode, Is.True);

                Assert.That(controlPoints.SamplePointAt(500).SampleBank, Is.EqualTo("drum"));
                Assert.That(controlPoints.SamplePointAt(1500).SampleBank, Is.EqualTo("drum"));
                Assert.That(controlPoints.SamplePointAt(2500).SampleBank, Is.EqualTo("normal"));
                Assert.That(controlPoints.SamplePointAt(3500).SampleBank, Is.EqualTo("drum"));

                Assert.That(controlPoints.TimingPointAt(500).BeatLength, Is.EqualTo(500).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(1500).BeatLength, Is.EqualTo(500).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(2500).BeatLength, Is.EqualTo(250).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(3500).BeatLength, Is.EqualTo(500).Within(0.1));
            }
        }

        [Test]
        public void TestTimingPointResetsSpeedMultiplier()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("timingpoint-speedmultiplier-reset.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.DifficultyPointAt(0).SpeedMultiplier, Is.EqualTo(0.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(2000).SpeedMultiplier, Is.EqualTo(1).Within(0.1));
            }
        }

        [Test]
        public void TestDecodeBeatmapColours()
        {
            var decoder = new LegacySkinDecoder();

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
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
                    new Color4(100, 100, 100, 100),
                };
                Assert.AreEqual(expectedColors.Length, comboColors.Count);
                for (int i = 0; i < expectedColors.Length; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        [Test]
        public void TestDecodeBeatmapComboOffsetsOsu()
        {
            var decoder = new LegacyBeatmapDecoder();

            using (var resStream = TestResources.OpenResource("hitobject-combo-offset.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);

                var converted = new OsuBeatmapConverter(beatmap).Convert();
                new OsuBeatmapProcessor(converted).PreProcess();
                new OsuBeatmapProcessor(converted).PostProcess();

                Assert.AreEqual(4, ((IHasComboInformation)converted.HitObjects.ElementAt(0)).ComboIndex);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(2)).ComboIndex);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(4)).ComboIndex);
                Assert.AreEqual(6, ((IHasComboInformation)converted.HitObjects.ElementAt(6)).ComboIndex);
                Assert.AreEqual(11, ((IHasComboInformation)converted.HitObjects.ElementAt(8)).ComboIndex);
                Assert.AreEqual(14, ((IHasComboInformation)converted.HitObjects.ElementAt(11)).ComboIndex);
            }
        }

        [Test]
        public void TestDecodeBeatmapComboOffsetsCatch()
        {
            var decoder = new LegacyBeatmapDecoder();

            using (var resStream = TestResources.OpenResource("hitobject-combo-offset.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);

                var converted = new CatchBeatmapConverter(beatmap).Convert();
                new CatchBeatmapProcessor(converted).PreProcess();
                new CatchBeatmapProcessor(converted).PostProcess();

                Assert.AreEqual(4, ((IHasComboInformation)converted.HitObjects.ElementAt(0)).ComboIndex);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(2)).ComboIndex);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(4)).ComboIndex);
                Assert.AreEqual(6, ((IHasComboInformation)converted.HitObjects.ElementAt(6)).ComboIndex);
                Assert.AreEqual(11, ((IHasComboInformation)converted.HitObjects.ElementAt(8)).ComboIndex);
                Assert.AreEqual(14, ((IHasComboInformation)converted.HitObjects.ElementAt(11)).ComboIndex);
            }
        }

        [Test]
        public void TestDecodeBeatmapHitObjects()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                var curveData = hitObjects[0] as IHasCurve;
                var positionData = hitObjects[0] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.IsNotNull(curveData);
                Assert.AreEqual(new Vector2(192, 168), positionData.Position);
                Assert.AreEqual(956, hitObjects[0].StartTime);
                Assert.IsTrue(hitObjects[0].Samples.Any(s => s.Name == HitSampleInfo.HIT_NORMAL));

                positionData = hitObjects[1] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.AreEqual(new Vector2(304, 56), positionData.Position);
                Assert.AreEqual(1285, hitObjects[1].StartTime);
                Assert.IsTrue(hitObjects[1].Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP));
            }
        }

        [Test]
        public void TestDecodeControlPointDifficultyChange()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("controlpoint-difficulty-multiplier.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPointInfo = decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPointInfo.DifficultyPointAt(5).SpeedMultiplier, Is.EqualTo(1));
                Assert.That(controlPointInfo.DifficultyPointAt(1000).SpeedMultiplier, Is.EqualTo(10));
                Assert.That(controlPointInfo.DifficultyPointAt(2000).SpeedMultiplier, Is.EqualTo(1.8518518518518519d));
                Assert.That(controlPointInfo.DifficultyPointAt(3000).SpeedMultiplier, Is.EqualTo(0.5));
            }
        }

        [Test]
        public void TestDecodeControlPointCustomSampleBank()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("controlpoint-custom-samplebank.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                Assert.AreEqual("normal-hitnormal", getTestableSampleInfo(hitObjects[0]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal", getTestableSampleInfo(hitObjects[1]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal2", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal", getTestableSampleInfo(hitObjects[3]).LookupNames.First());

                // The control point at the end time of the slider should be applied
                Assert.AreEqual("soft-hitnormal8", getTestableSampleInfo(hitObjects[4]).LookupNames.First());
            }

            HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.SampleControlPoint.ApplyTo(hitObject.Samples[0]);
        }

        [Test]
        public void TestDecodeHitObjectCustomSampleBank()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("hitobject-custom-samplebank.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                Assert.AreEqual("normal-hitnormal", getTestableSampleInfo(hitObjects[0]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal2", getTestableSampleInfo(hitObjects[1]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal3", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
            }

            HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.SampleControlPoint.ApplyTo(hitObject.Samples[0]);
        }

        [Test]
        public void TestDecodeHitObjectFileSamples()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("hitobject-file-samples.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                Assert.AreEqual("hit_1.wav", getTestableSampleInfo(hitObjects[0]).LookupNames.First());
                Assert.AreEqual("hit_2.wav", getTestableSampleInfo(hitObjects[1]).LookupNames.First());
                Assert.AreEqual("normal-hitnormal2", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
                Assert.AreEqual("hit_1.wav", getTestableSampleInfo(hitObjects[3]).LookupNames.First());
                Assert.AreEqual(70, getTestableSampleInfo(hitObjects[3]).Volume);
            }

            HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.SampleControlPoint.ApplyTo(hitObject.Samples[0]);
        }

        [Test]
        public void TestDecodeSliderSamples()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("slider-samples.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                var slider1 = (ConvertSlider)hitObjects[0];

                Assert.AreEqual(1, slider1.NodeSamples[0].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider1.NodeSamples[0][0].Name);
                Assert.AreEqual(1, slider1.NodeSamples[1].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider1.NodeSamples[1][0].Name);
                Assert.AreEqual(1, slider1.NodeSamples[2].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider1.NodeSamples[2][0].Name);

                var slider2 = (ConvertSlider)hitObjects[1];

                Assert.AreEqual(2, slider2.NodeSamples[0].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider2.NodeSamples[0][0].Name);
                Assert.AreEqual(HitSampleInfo.HIT_CLAP, slider2.NodeSamples[0][1].Name);
                Assert.AreEqual(2, slider2.NodeSamples[1].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider2.NodeSamples[1][0].Name);
                Assert.AreEqual(HitSampleInfo.HIT_CLAP, slider2.NodeSamples[1][1].Name);
                Assert.AreEqual(2, slider2.NodeSamples[2].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider2.NodeSamples[2][0].Name);
                Assert.AreEqual(HitSampleInfo.HIT_CLAP, slider2.NodeSamples[2][1].Name);

                var slider3 = (ConvertSlider)hitObjects[2];

                Assert.AreEqual(2, slider3.NodeSamples[0].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider3.NodeSamples[0][0].Name);
                Assert.AreEqual(HitSampleInfo.HIT_WHISTLE, slider3.NodeSamples[0][1].Name);
                Assert.AreEqual(1, slider3.NodeSamples[1].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider3.NodeSamples[1][0].Name);
                Assert.AreEqual(2, slider3.NodeSamples[2].Count);
                Assert.AreEqual(HitSampleInfo.HIT_NORMAL, slider3.NodeSamples[2][0].Name);
                Assert.AreEqual(HitSampleInfo.HIT_CLAP, slider3.NodeSamples[2][1].Name);
            }
        }

        [Test]
        public void TestDecodeHitObjectNullAdditionBank()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("hitobject-no-addition-bank.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                Assert.AreEqual(hitObjects[0].Samples[0].Bank, hitObjects[0].Samples[1].Bank);
            }
        }

        [Test]
        public void TestInvalidEventStillPasses()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var badResStream = TestResources.OpenResource("invalid-events.osu"))
            using (var badStream = new LineBufferedReader(badResStream))
            {
                Assert.DoesNotThrow(() => decoder.Decode(badStream));
            }
        }

        [Test]
        public void TestFallbackDecoderForCorruptedHeader()
        {
            Decoder<Beatmap> decoder = null;
            Beatmap beatmap = null;

            using (var resStream = TestResources.OpenResource("corrupted-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Beatmap with corrupted header", beatmap.Metadata.Title);
                Assert.AreEqual("Evil Hacker", beatmap.Metadata.AuthorString);
            }
        }

        [Test]
        public void TestFallbackDecoderForMissingHeader()
        {
            Decoder<Beatmap> decoder = null;
            Beatmap beatmap = null;

            using (var resStream = TestResources.OpenResource("missing-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Beatmap with no header", beatmap.Metadata.Title);
                Assert.AreEqual("Incredibly Evil Hacker", beatmap.Metadata.AuthorString);
            }
        }

        [Test]
        public void TestDecodeFileWithEmptyLinesAtStart()
        {
            Decoder<Beatmap> decoder = null;
            Beatmap beatmap = null;

            using (var resStream = TestResources.OpenResource("empty-lines-at-start.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Empty lines at start", beatmap.Metadata.Title);
                Assert.AreEqual("Edge Case Hunter", beatmap.Metadata.AuthorString);
            }
        }

        [Test]
        public void TestDecodeFileWithEmptyLinesAndNoHeader()
        {
            Decoder<Beatmap> decoder = null;
            Beatmap beatmap = null;

            using (var resStream = TestResources.OpenResource("empty-line-instead-of-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("The dog ate the file header", beatmap.Metadata.Title);
                Assert.AreEqual("Why does this keep happening", beatmap.Metadata.AuthorString);
            }
        }

        [Test]
        public void TestDecodeFileWithContentImmediatelyAfterHeader()
        {
            Decoder<Beatmap> decoder = null;
            Beatmap beatmap = null;

            using (var resStream = TestResources.OpenResource("no-empty-line-after-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("No empty line delimiting header from contents", beatmap.Metadata.Title);
                Assert.AreEqual("Edge Case Hunter", beatmap.Metadata.AuthorString);
            }
        }

        [Test]
        public void TestDecodeEmptyFile()
        {
            using (var resStream = new MemoryStream())
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.Throws<IOException>(() => Decoder.GetDecoder<Beatmap>(stream));
            }
        }

        [Test]
        public void TestAllowFallbackDecoderOverwrite()
        {
            Decoder<Beatmap> decoder = null;

            using (var resStream = TestResources.OpenResource("corrupted-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
            }

            Assert.DoesNotThrow(LegacyDifficultyCalculatorBeatmapDecoder.Register);

            using (var resStream = TestResources.OpenResource("corrupted-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyDifficultyCalculatorBeatmapDecoder>(decoder);
            }
        }
    }
}
