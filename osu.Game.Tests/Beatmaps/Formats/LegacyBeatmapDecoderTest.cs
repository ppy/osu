// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapDecoderTest
    {
        [Test]
        public void TestDecodeBeatmapVersion()
        {
            using (var resStream = TestResources.OpenResource("beatmap-version-6.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);
                var working = new TestWorkingBeatmap(decoder.Decode(stream));

                Assert.AreEqual(6, working.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(6, working.Beatmap.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(6, working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo, Array.Empty<Mod>()).BeatmapInfo.BeatmapVersion);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestPreviewPointWithOffsets(bool applyOffsets)
        {
            using (var resStream = TestResources.OpenResource("beatmap-version-4.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);
                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = applyOffsets;
                var working = new TestWorkingBeatmap(decoder.Decode(stream));

                Assert.AreEqual(4, working.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(4, working.Beatmap.BeatmapInfo.BeatmapVersion);
                Assert.AreEqual(4, working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo, Array.Empty<Mod>()).BeatmapInfo.BeatmapVersion);

                Assert.AreEqual(-1, working.BeatmapInfo.Metadata.PreviewTime);
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
                Assert.AreEqual(0.7f, beatmapInfo.StackLeniency);
                Assert.IsTrue(beatmapInfo.Ruleset.OnlineID == 0);
                Assert.IsFalse(beatmapInfo.LetterboxInBreaks);
                Assert.IsFalse(beatmapInfo.SpecialStyle);
                Assert.IsFalse(beatmapInfo.WidescreenStoryboard);
                Assert.IsFalse(beatmapInfo.SamplesMatchPlaybackRate);
                Assert.AreEqual(CountdownType.None, beatmapInfo.Countdown);
                Assert.AreEqual(0, beatmapInfo.CountdownOffset);
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
                Assert.AreEqual("Gamu", metadata.Author.Username);
                Assert.AreEqual("Insane", beatmapInfo.DifficultyName);
                Assert.AreEqual(string.Empty, metadata.Source);
                Assert.AreEqual("MBC7 Unisphere 地球ヤバイEP Chikyu Yabai", metadata.Tags);
                Assert.AreEqual(557821, beatmapInfo.OnlineID);
                Assert.AreEqual(241526, beatmapInfo.BeatmapSet?.OnlineID);
            }
        }

        [Test]
        public void TestDecodeBeatmapDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder();

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var difficulty = decoder.Decode(stream).Difficulty;

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
        public void TestDecodeVideoWithLowercaseExtension()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("video-with-lowercase-extension.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var metadata = beatmap.Metadata;

                Assert.AreEqual("BG.jpg", metadata.BackgroundFile);
            }
        }

        [Test]
        public void TestDecodeVideoWithUppercaseExtension()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("video-with-uppercase-extension.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var metadata = beatmap.Metadata;

                Assert.AreEqual("BG.jpg", metadata.BackgroundFile);
            }
        }

        [Test]
        public void TestDecodeImageSpecifiedAsVideo()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("image-specified-as-video.osb"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                var metadata = beatmap.Metadata;

                Assert.AreEqual("BG.jpg", metadata.BackgroundFile);
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
                var controlPoints = (LegacyControlPointInfo)beatmap.ControlPointInfo;

                Assert.AreEqual(4, controlPoints.TimingPoints.Count);
                Assert.AreEqual(5, controlPoints.DifficultyPoints.Count);
                Assert.AreEqual(34, controlPoints.SamplePoints.Count);
                Assert.AreEqual(8, controlPoints.EffectPoints.Count);

                var timingPoint = controlPoints.TimingPointAt(0);
                Assert.AreEqual(956, timingPoint.Time);
                Assert.AreEqual(329.67032967033, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignature.SimpleQuadruple, timingPoint.TimeSignature);
                Assert.IsFalse(timingPoint.OmitFirstBarLine);

                timingPoint = controlPoints.TimingPointAt(48428);
                Assert.AreEqual(956, timingPoint.Time);
                Assert.AreEqual(329.67032967033d, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignature.SimpleQuadruple, timingPoint.TimeSignature);
                Assert.IsFalse(timingPoint.OmitFirstBarLine);

                timingPoint = controlPoints.TimingPointAt(119637);
                Assert.AreEqual(119637, timingPoint.Time);
                Assert.AreEqual(659.340659340659, timingPoint.BeatLength);
                Assert.AreEqual(TimeSignature.SimpleQuadruple, timingPoint.TimeSignature);
                Assert.IsFalse(timingPoint.OmitFirstBarLine);

                var difficultyPoint = controlPoints.DifficultyPointAt(0);
                Assert.AreEqual(0, difficultyPoint.Time);
                Assert.AreEqual(1.0, difficultyPoint.SliderVelocity);

                difficultyPoint = controlPoints.DifficultyPointAt(48428);
                Assert.AreEqual(0, difficultyPoint.Time);
                Assert.AreEqual(1.0, difficultyPoint.SliderVelocity);

                difficultyPoint = controlPoints.DifficultyPointAt(116999);
                Assert.AreEqual(116999, difficultyPoint.Time);
                Assert.AreEqual(0.75, difficultyPoint.SliderVelocity, 0.1);

                var soundPoint = controlPoints.SamplePointAt(0);
                Assert.AreEqual(956, soundPoint.Time);
                Assert.AreEqual(HitSampleInfo.BANK_SOFT, soundPoint.SampleBank);
                Assert.AreEqual(60, soundPoint.SampleVolume);

                soundPoint = controlPoints.SamplePointAt(53373);
                Assert.AreEqual(53373, soundPoint.Time);
                Assert.AreEqual(HitSampleInfo.BANK_SOFT, soundPoint.SampleBank);
                Assert.AreEqual(60, soundPoint.SampleVolume);

                soundPoint = controlPoints.SamplePointAt(119637);
                Assert.AreEqual(119637, soundPoint.Time);
                Assert.AreEqual(HitSampleInfo.BANK_SOFT, soundPoint.SampleBank);
                Assert.AreEqual(80, soundPoint.SampleVolume);

                var effectPoint = controlPoints.EffectPointAt(0);
                Assert.AreEqual(0, effectPoint.Time);
                Assert.IsFalse(effectPoint.KiaiMode);

                effectPoint = controlPoints.EffectPointAt(53703);
                Assert.AreEqual(53703, effectPoint.Time);
                Assert.IsTrue(effectPoint.KiaiMode);

                effectPoint = controlPoints.EffectPointAt(116637);
                Assert.AreEqual(95901, effectPoint.Time);
                Assert.IsFalse(effectPoint.KiaiMode);
            }
        }

        [Test]
        public void TestDecodeOverlappingTimingPoints()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("overlapping-control-points.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = (LegacyControlPointInfo)decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.TimingPoints.Count, Is.EqualTo(4));
                Assert.That(controlPoints.DifficultyPoints.Count, Is.EqualTo(3));
                Assert.That(controlPoints.EffectPoints.Count, Is.EqualTo(3));
                Assert.That(controlPoints.SamplePoints.Count, Is.EqualTo(3));

                Assert.That(controlPoints.DifficultyPointAt(500).SliderVelocity, Is.EqualTo(1.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(1500).SliderVelocity, Is.EqualTo(1.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(2500).SliderVelocity, Is.EqualTo(0.75).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(3500).SliderVelocity, Is.EqualTo(1.5).Within(0.1));

                Assert.That(controlPoints.EffectPointAt(500).KiaiMode, Is.True);
                Assert.That(controlPoints.EffectPointAt(1500).KiaiMode, Is.True);
                Assert.That(controlPoints.EffectPointAt(2500).KiaiMode, Is.False);
                Assert.That(controlPoints.EffectPointAt(3500).KiaiMode, Is.True);

                Assert.That(controlPoints.SamplePointAt(500).SampleBank, Is.EqualTo(HitSampleInfo.BANK_DRUM));
                Assert.That(controlPoints.SamplePointAt(1500).SampleBank, Is.EqualTo(HitSampleInfo.BANK_DRUM));
                Assert.That(controlPoints.SamplePointAt(2500).SampleBank, Is.EqualTo(HitSampleInfo.BANK_NORMAL));
                Assert.That(controlPoints.SamplePointAt(3500).SampleBank, Is.EqualTo(HitSampleInfo.BANK_DRUM));

                Assert.That(controlPoints.TimingPointAt(500).BeatLength, Is.EqualTo(500).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(1500).BeatLength, Is.EqualTo(500).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(2500).BeatLength, Is.EqualTo(250).Within(0.1));
                Assert.That(controlPoints.TimingPointAt(3500).BeatLength, Is.EqualTo(500).Within(0.1));
            }
        }

        [Test]
        public void TestDecodeOmitBarLineEffect()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("omit-barline-control-points.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = (LegacyControlPointInfo)decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.TimingPoints.Count, Is.EqualTo(6));
                Assert.That(controlPoints.EffectPoints.Count, Is.EqualTo(0));

                Assert.That(controlPoints.TimingPointAt(500).OmitFirstBarLine, Is.False);
                Assert.That(controlPoints.TimingPointAt(1500).OmitFirstBarLine, Is.True);
                Assert.That(controlPoints.TimingPointAt(2500).OmitFirstBarLine, Is.False);
                Assert.That(controlPoints.TimingPointAt(3500).OmitFirstBarLine, Is.False);
                Assert.That(controlPoints.TimingPointAt(4500).OmitFirstBarLine, Is.False);
                Assert.That(controlPoints.TimingPointAt(5500).OmitFirstBarLine, Is.True);
            }
        }

        [Test]
        public void TestTimingPointResetsSpeedMultiplier()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("timingpoint-speedmultiplier-reset.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = (LegacyControlPointInfo)decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.DifficultyPointAt(0).SliderVelocity, Is.EqualTo(0.5).Within(0.1));
                Assert.That(controlPoints.DifficultyPointAt(2000).SliderVelocity, Is.EqualTo(1).Within(0.1));
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

                Debug.Assert(comboColors != null);

                Color4[] expectedColors =
                {
                    new Color4(142, 199, 255, 255),
                    new Color4(255, 128, 128, 255),
                    new Color4(128, 255, 255, 255),
                    new Color4(128, 255, 128, 255),
                    new Color4(255, 187, 255, 255),
                    new Color4(255, 177, 140, 255),
                    new Color4(100, 100, 100, 255), // alpha is specified as 100, but should be ignored.
                };
                Assert.AreEqual(expectedColors.Length, comboColors.Count);
                for (int i = 0; i < expectedColors.Length; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        [Test]
        public void TestGetLastObjectTime()
        {
            var decoder = new LegacyBeatmapDecoder();

            using (var resStream = TestResources.OpenResource("mania-last-object-not-latest.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);

                Assert.That(beatmap.HitObjects.Last().StartTime, Is.EqualTo(2494));
                Assert.That(beatmap.HitObjects.Last().GetEndTime(), Is.EqualTo(2494));

                Assert.That(beatmap.HitObjects.Max(h => h.GetEndTime()), Is.EqualTo(2582));
                Assert.That(beatmap.GetLastObjectTime(), Is.EqualTo(2582));
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

                var converted = new OsuBeatmapConverter(beatmap, new OsuRuleset()).Convert();
                new OsuBeatmapProcessor(converted).PreProcess();
                new OsuBeatmapProcessor(converted).PostProcess();

                Assert.AreEqual(4, ((IHasComboInformation)converted.HitObjects.ElementAt(0)).ComboIndexWithOffsets);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(2)).ComboIndexWithOffsets);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(4)).ComboIndexWithOffsets);
                Assert.AreEqual(6, ((IHasComboInformation)converted.HitObjects.ElementAt(6)).ComboIndexWithOffsets);
                Assert.AreEqual(11, ((IHasComboInformation)converted.HitObjects.ElementAt(8)).ComboIndexWithOffsets);
                Assert.AreEqual(14, ((IHasComboInformation)converted.HitObjects.ElementAt(11)).ComboIndexWithOffsets);
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

                var converted = new CatchBeatmapConverter(beatmap, new CatchRuleset()).Convert();
                new CatchBeatmapProcessor(converted).PreProcess();
                new CatchBeatmapProcessor(converted).PostProcess();

                Assert.AreEqual(4, ((IHasComboInformation)converted.HitObjects.ElementAt(0)).ComboIndexWithOffsets);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(2)).ComboIndexWithOffsets);
                Assert.AreEqual(5, ((IHasComboInformation)converted.HitObjects.ElementAt(4)).ComboIndexWithOffsets);
                Assert.AreEqual(6, ((IHasComboInformation)converted.HitObjects.ElementAt(6)).ComboIndexWithOffsets);
                Assert.AreEqual(11, ((IHasComboInformation)converted.HitObjects.ElementAt(8)).ComboIndexWithOffsets);
                Assert.AreEqual(14, ((IHasComboInformation)converted.HitObjects.ElementAt(11)).ComboIndexWithOffsets);
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

                var curveData = hitObjects[0] as IHasPathWithRepeats;
                var positionData = hitObjects[0] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.IsNotNull(curveData);
                Assert.AreEqual(new Vector2(192, 168), positionData!.Position);
                Assert.AreEqual(956, hitObjects[0].StartTime);
                Assert.IsTrue(hitObjects[0].Samples.Any(s => s.Name == HitSampleInfo.HIT_NORMAL));

                positionData = hitObjects[1] as IHasPosition;

                Assert.IsNotNull(positionData);
                Assert.AreEqual(new Vector2(304, 56), positionData!.Position);
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
                var controlPointInfo = (LegacyControlPointInfo)decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPointInfo.DifficultyPointAt(5).SliderVelocity, Is.EqualTo(1));
                Assert.That(controlPointInfo.DifficultyPointAt(1000).SliderVelocity, Is.EqualTo(10));
                Assert.That(controlPointInfo.DifficultyPointAt(2000).SliderVelocity, Is.EqualTo(1.8518518518518519d));
                Assert.That(controlPointInfo.DifficultyPointAt(3000).SliderVelocity, Is.EqualTo(0.5));
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

                Assert.AreEqual("Gameplay/normal-hitnormal", getTestableSampleInfo(hitObjects[0]).LookupNames.First());
                Assert.AreEqual("Gameplay/normal-hitnormal", getTestableSampleInfo(hitObjects[1]).LookupNames.First());
                Assert.AreEqual("Gameplay/normal-hitnormal2", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
                Assert.AreEqual("Gameplay/normal-hitnormal", getTestableSampleInfo(hitObjects[3]).LookupNames.First());

                // The control point at the end time of the slider should be applied
                Assert.AreEqual("Gameplay/soft-hitnormal8", getTestableSampleInfo(hitObjects[4]).LookupNames.First());
            }

            static HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.Samples[0];
        }

        [Test]
        public void TestDecodeHitObjectCustomSampleBank()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("hitobject-custom-samplebank.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                Assert.AreEqual("Gameplay/normal-hitnormal", getTestableSampleInfo(hitObjects[0]).LookupNames.First());
                Assert.AreEqual("Gameplay/normal-hitnormal2", getTestableSampleInfo(hitObjects[1]).LookupNames.First());
                Assert.AreEqual("Gameplay/normal-hitnormal3", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
            }

            static HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.Samples[0];
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
                Assert.AreEqual("Gameplay/normal-hitnormal2", getTestableSampleInfo(hitObjects[2]).LookupNames.First());
                Assert.AreEqual("hit_1.wav", getTestableSampleInfo(hitObjects[3]).LookupNames.First());
                Assert.AreEqual(70, getTestableSampleInfo(hitObjects[3]).Volume);
            }

            static HitSampleInfo getTestableSampleInfo(HitObject hitObject) => hitObject.Samples[0];
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
        public void TestInvalidBankDefaultsToNormal()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("invalid-bank.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObjects = decoder.Decode(stream).HitObjects;

                assertObjectHasBanks(hitObjects[0], HitSampleInfo.BANK_DRUM);
                assertObjectHasBanks(hitObjects[1], HitSampleInfo.BANK_NORMAL);
                assertObjectHasBanks(hitObjects[2], HitSampleInfo.BANK_SOFT);
                assertObjectHasBanks(hitObjects[3], HitSampleInfo.BANK_DRUM);
                assertObjectHasBanks(hitObjects[4], HitSampleInfo.BANK_NORMAL);

                assertObjectHasBanks(hitObjects[5], HitSampleInfo.BANK_DRUM, HitSampleInfo.BANK_DRUM);
                assertObjectHasBanks(hitObjects[6], HitSampleInfo.BANK_DRUM, HitSampleInfo.BANK_NORMAL);
                assertObjectHasBanks(hitObjects[7], HitSampleInfo.BANK_DRUM, HitSampleInfo.BANK_SOFT);
                assertObjectHasBanks(hitObjects[8], HitSampleInfo.BANK_DRUM, HitSampleInfo.BANK_DRUM);
                assertObjectHasBanks(hitObjects[9], HitSampleInfo.BANK_DRUM, HitSampleInfo.BANK_NORMAL);
            }

            static void assertObjectHasBanks(HitObject hitObject, string normalBank, string? additionsBank = null)
            {
                Assert.AreEqual(normalBank, hitObject.Samples[0].Bank);

                if (additionsBank != null)
                    Assert.AreEqual(additionsBank, hitObject.Samples[1].Bank);
            }
        }

        [Test]
        public void TestFallbackDecoderForCorruptedHeader()
        {
            Decoder<Beatmap> decoder = null!;
            Beatmap beatmap = null!;

            using (var resStream = TestResources.OpenResource("corrupted-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Beatmap with corrupted header", beatmap.Metadata.Title);
                Assert.AreEqual("Evil Hacker", beatmap.Metadata.Author.Username);
            }
        }

        [Test]
        public void TestFallbackDecoderForMissingHeader()
        {
            Decoder<Beatmap> decoder = null!;
            Beatmap beatmap = null!;

            using (var resStream = TestResources.OpenResource("missing-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Beatmap with no header", beatmap.Metadata.Title);
                Assert.AreEqual("Incredibly Evil Hacker", beatmap.Metadata.Author.Username);
            }
        }

        [Test]
        public void TestDecodeFileWithEmptyLinesAtStart()
        {
            Decoder<Beatmap> decoder = null!;
            Beatmap beatmap = null!;

            using (var resStream = TestResources.OpenResource("empty-lines-at-start.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("Empty lines at start", beatmap.Metadata.Title);
                Assert.AreEqual("Edge Case Hunter", beatmap.Metadata.Author.Username);
            }
        }

        [Test]
        public void TestDecodeFileWithEmptyLinesAndNoHeader()
        {
            Decoder<Beatmap> decoder = null!;
            Beatmap beatmap = null!;

            using (var resStream = TestResources.OpenResource("empty-line-instead-of-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("The dog ate the file header", beatmap.Metadata.Title);
                Assert.AreEqual("Why does this keep happening", beatmap.Metadata.Author.Username);
            }
        }

        [Test]
        public void TestDecodeFileWithContentImmediatelyAfterHeader()
        {
            Decoder<Beatmap> decoder = null!;
            Beatmap beatmap = null!;

            using (var resStream = TestResources.OpenResource("no-empty-line-after-header.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Assert.DoesNotThrow(() => decoder = Decoder.GetDecoder<Beatmap>(stream));
                Assert.IsInstanceOf<LegacyBeatmapDecoder>(decoder);
                Assert.DoesNotThrow(() => beatmap = decoder.Decode(stream));
                Assert.IsNotNull(beatmap);
                Assert.AreEqual("No empty line delimiting header from contents", beatmap.Metadata.Title);
                Assert.AreEqual("Edge Case Hunter", beatmap.Metadata.Author.Username);
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
            Decoder<Beatmap> decoder = null!;

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

        [Test]
        public void TestMultiSegmentSliders()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("multi-segment-slider.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);

                // Multi-segment
                var first = ((IHasPath)decoded.HitObjects[0]).Path;

                Assert.That(first.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(first.ControlPoints[0].Type, Is.EqualTo(PathType.PERFECT_CURVE));
                Assert.That(first.ControlPoints[1].Position, Is.EqualTo(new Vector2(161, -244)));
                Assert.That(first.ControlPoints[1].Type, Is.EqualTo(null));

                // ReSharper disable once HeuristicUnreachableCode
                // weird one, see https://youtrack.jetbrains.com/issue/RIDER-70159.
                Assert.That(first.ControlPoints[2].Position, Is.EqualTo(new Vector2(376, -3)));
                Assert.That(first.ControlPoints[2].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(first.ControlPoints[3].Position, Is.EqualTo(new Vector2(68, 15)));
                Assert.That(first.ControlPoints[3].Type, Is.EqualTo(null));
                Assert.That(first.ControlPoints[4].Position, Is.EqualTo(new Vector2(259, -132)));
                Assert.That(first.ControlPoints[4].Type, Is.EqualTo(null));
                Assert.That(first.ControlPoints[5].Position, Is.EqualTo(new Vector2(92, -107)));
                Assert.That(first.ControlPoints[5].Type, Is.EqualTo(null));

                // Single-segment
                var second = ((IHasPath)decoded.HitObjects[1]).Path;

                Assert.That(second.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(second.ControlPoints[0].Type, Is.EqualTo(PathType.PERFECT_CURVE));
                Assert.That(second.ControlPoints[1].Position, Is.EqualTo(new Vector2(161, -244)));
                Assert.That(second.ControlPoints[1].Type, Is.EqualTo(null));
                Assert.That(second.ControlPoints[2].Position, Is.EqualTo(new Vector2(376, -3)));
                Assert.That(second.ControlPoints[2].Type, Is.EqualTo(null));

                // Implicit multi-segment
                var third = ((IHasPath)decoded.HitObjects[2]).Path;

                Assert.That(third.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(third.ControlPoints[0].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(third.ControlPoints[1].Position, Is.EqualTo(new Vector2(0, 192)));
                Assert.That(third.ControlPoints[1].Type, Is.EqualTo(null));
                Assert.That(third.ControlPoints[2].Position, Is.EqualTo(new Vector2(224, 192)));
                Assert.That(third.ControlPoints[2].Type, Is.EqualTo(null));

                Assert.That(third.ControlPoints[3].Position, Is.EqualTo(new Vector2(224, 0)));
                Assert.That(third.ControlPoints[3].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(third.ControlPoints[4].Position, Is.EqualTo(new Vector2(224, -192)));
                Assert.That(third.ControlPoints[4].Type, Is.EqualTo(null));
                Assert.That(third.ControlPoints[5].Position, Is.EqualTo(new Vector2(480, -192)));
                Assert.That(third.ControlPoints[5].Type, Is.EqualTo(null));
                Assert.That(third.ControlPoints[6].Position, Is.EqualTo(new Vector2(480, 0)));
                Assert.That(third.ControlPoints[6].Type, Is.EqualTo(null));

                // Last control point duplicated
                var fourth = ((IHasPath)decoded.HitObjects[3]).Path;

                Assert.That(fourth.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(fourth.ControlPoints[0].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(fourth.ControlPoints[1].Position, Is.EqualTo(new Vector2(1, 1)));
                Assert.That(fourth.ControlPoints[1].Type, Is.EqualTo(null));
                Assert.That(fourth.ControlPoints[2].Position, Is.EqualTo(new Vector2(2, 2)));
                Assert.That(fourth.ControlPoints[2].Type, Is.EqualTo(null));
                Assert.That(fourth.ControlPoints[3].Position, Is.EqualTo(new Vector2(3, 3)));
                Assert.That(fourth.ControlPoints[3].Type, Is.EqualTo(null));
                Assert.That(fourth.ControlPoints[4].Position, Is.EqualTo(new Vector2(3, 3)));
                Assert.That(fourth.ControlPoints[4].Type, Is.EqualTo(null));

                // Last control point in segment duplicated
                var fifth = ((IHasPath)decoded.HitObjects[4]).Path;

                Assert.That(fifth.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(fifth.ControlPoints[0].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(fifth.ControlPoints[1].Position, Is.EqualTo(new Vector2(1, 1)));
                Assert.That(fifth.ControlPoints[1].Type, Is.EqualTo(null));
                Assert.That(fifth.ControlPoints[2].Position, Is.EqualTo(new Vector2(2, 2)));
                Assert.That(fifth.ControlPoints[2].Type, Is.EqualTo(null));
                Assert.That(fifth.ControlPoints[3].Position, Is.EqualTo(new Vector2(3, 3)));
                Assert.That(fifth.ControlPoints[3].Type, Is.EqualTo(null));
                Assert.That(fifth.ControlPoints[4].Position, Is.EqualTo(new Vector2(3, 3)));
                Assert.That(fifth.ControlPoints[4].Type, Is.EqualTo(null));

                Assert.That(fifth.ControlPoints[5].Position, Is.EqualTo(new Vector2(4, 4)));
                Assert.That(fifth.ControlPoints[5].Type, Is.EqualTo(PathType.BEZIER));
                Assert.That(fifth.ControlPoints[6].Position, Is.EqualTo(new Vector2(5, 5)));
                Assert.That(fifth.ControlPoints[6].Type, Is.EqualTo(null));

                // Implicit perfect-curve multi-segment(Should convert to bezier to match stable)
                var sixth = ((IHasPath)decoded.HitObjects[5]).Path;

                Assert.That(sixth.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(sixth.ControlPoints[0].Type == PathType.BEZIER);
                Assert.That(sixth.ControlPoints[1].Position, Is.EqualTo(new Vector2(75, 145)));
                Assert.That(sixth.ControlPoints[1].Type == null);
                Assert.That(sixth.ControlPoints[2].Position, Is.EqualTo(new Vector2(170, 75)));

                Assert.That(sixth.ControlPoints[2].Type == PathType.BEZIER);
                Assert.That(sixth.ControlPoints[3].Position, Is.EqualTo(new Vector2(300, 145)));
                Assert.That(sixth.ControlPoints[3].Type == null);
                Assert.That(sixth.ControlPoints[4].Position, Is.EqualTo(new Vector2(410, 20)));
                Assert.That(sixth.ControlPoints[4].Type == null);

                // Explicit perfect-curve multi-segment(Should not convert to bezier)
                var seventh = ((IHasPath)decoded.HitObjects[6]).Path;

                Assert.That(seventh.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(seventh.ControlPoints[0].Type == PathType.PERFECT_CURVE);
                Assert.That(seventh.ControlPoints[1].Position, Is.EqualTo(new Vector2(75, 145)));
                Assert.That(seventh.ControlPoints[1].Type == null);
                Assert.That(seventh.ControlPoints[2].Position, Is.EqualTo(new Vector2(170, 75)));

                Assert.That(seventh.ControlPoints[2].Type == PathType.PERFECT_CURVE);
                Assert.That(seventh.ControlPoints[3].Position, Is.EqualTo(new Vector2(300, 145)));
                Assert.That(seventh.ControlPoints[3].Type == null);
                Assert.That(seventh.ControlPoints[4].Position, Is.EqualTo(new Vector2(410, 20)));
                Assert.That(seventh.ControlPoints[4].Type == null);
            }
        }

        [Test]
        public void TestSliderLengthExtensionEdgeCase()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("duplicate-last-position-slider.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);

                var path = ((IHasPath)decoded.HitObjects[0]).Path;

                Assert.That(path.ExpectedDistance.Value, Is.EqualTo(2));
                Assert.That(path.Distance, Is.EqualTo(1));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestLegacyDefaultsPreserved(bool applyOffsets)
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = applyOffsets };

            using (var memoryStream = new MemoryStream())
            using (var stream = new LineBufferedReader(memoryStream))
            {
                var decoded = decoder.Decode(stream);

                Assert.Multiple(() =>
                {
                    Assert.That(decoded.BeatmapInfo.AudioLeadIn, Is.EqualTo(0));
                    Assert.That(decoded.BeatmapInfo.StackLeniency, Is.EqualTo(0.7f));
                    Assert.That(decoded.BeatmapInfo.SpecialStyle, Is.False);
                    Assert.That(decoded.BeatmapInfo.LetterboxInBreaks, Is.False);
                    Assert.That(decoded.BeatmapInfo.WidescreenStoryboard, Is.False);
                    Assert.That(decoded.BeatmapInfo.EpilepsyWarning, Is.False);
                    Assert.That(decoded.BeatmapInfo.SamplesMatchPlaybackRate, Is.False);
                    Assert.That(decoded.BeatmapInfo.Countdown, Is.EqualTo(CountdownType.Normal));
                    Assert.That(decoded.BeatmapInfo.CountdownOffset, Is.EqualTo(0));
                    Assert.That(decoded.BeatmapInfo.Metadata.PreviewTime, Is.EqualTo(-1));
                    Assert.That(decoded.BeatmapInfo.Ruleset.OnlineID, Is.EqualTo(0));
                });
            }
        }

        [Test]
        public void TestUndefinedApproachRateInheritsOverallDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("undefined-approach-rate.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                Assert.That(decoded.Difficulty.ApproachRate, Is.EqualTo(1));
                Assert.That(decoded.Difficulty.OverallDifficulty, Is.EqualTo(1));
            }
        }

        [Test]
        public void TestApproachRateDefinedBeforeOverallDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("approach-rate-before-overall-difficulty.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                Assert.That(decoded.Difficulty.ApproachRate, Is.EqualTo(9));
                Assert.That(decoded.Difficulty.OverallDifficulty, Is.EqualTo(1));
            }
        }

        [Test]
        public void TestApproachRateDefinedAfterOverallDifficulty()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("approach-rate-after-overall-difficulty.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                Assert.That(decoded.Difficulty.ApproachRate, Is.EqualTo(9));
                Assert.That(decoded.Difficulty.OverallDifficulty, Is.EqualTo(1));
            }
        }

        [Test]
        public void TestLegacyAdjacentImplicitCatmullSegmentsAreMerged()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("adjacent-catmull-segments.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                var controlPoints = ((IHasPath)decoded.HitObjects[0]).Path.ControlPoints;

                Assert.That(controlPoints.Count, Is.EqualTo(6));
                Assert.That(controlPoints.Single(c => c.Type != null).Type, Is.EqualTo(PathType.CATMULL));
            }
        }

        [Test]
        public void TestNonLegacyAdjacentImplicitCatmullSegmentsAreNotMerged()
        {
            var decoder = new LegacyBeatmapDecoder(LegacyBeatmapEncoder.FIRST_LAZER_VERSION) { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("adjacent-catmull-segments.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                var controlPoints = ((IHasPath)decoded.HitObjects[0]).Path.ControlPoints;

                Assert.That(controlPoints.Count, Is.EqualTo(4));
                Assert.That(controlPoints[0].Type, Is.EqualTo(PathType.CATMULL));
                Assert.That(controlPoints[1].Type, Is.EqualTo(PathType.CATMULL));
                Assert.That(controlPoints[2].Type, Is.EqualTo(PathType.CATMULL));
                Assert.That(controlPoints[3].Type, Is.Null);
            }
        }

        [Test]
        public void TestLegacyDuplicateInitialCatmullPointIsMerged()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("catmull-duplicate-initial-controlpoint.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoded = decoder.Decode(stream);
                var controlPoints = ((IHasPath)decoded.HitObjects[0]).Path.ControlPoints;

                Assert.That(controlPoints.Count, Is.EqualTo(4));
                Assert.That(controlPoints[0].Type, Is.EqualTo(PathType.CATMULL));
                Assert.That(controlPoints[0].Position, Is.EqualTo(Vector2.Zero));
                Assert.That(controlPoints[1].Type, Is.Null);
                Assert.That(controlPoints[1].Position, Is.Not.EqualTo(Vector2.Zero));
            }
        }

        [Test]
        public void TestNaNControlPoints()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("nan-control-points.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var controlPoints = (LegacyControlPointInfo)decoder.Decode(stream).ControlPointInfo;

                Assert.That(controlPoints.TimingPoints.Count, Is.EqualTo(1));
                Assert.That(controlPoints.DifficultyPoints.Count, Is.EqualTo(2));

                Assert.That(controlPoints.TimingPointAt(1000).BeatLength, Is.EqualTo(500));

                Assert.That(controlPoints.DifficultyPointAt(2000).SliderVelocity, Is.EqualTo(1));
                Assert.That(controlPoints.DifficultyPointAt(3000).SliderVelocity, Is.EqualTo(1));

                Assert.That(controlPoints.DifficultyPointAt(2000).GenerateTicks, Is.False);
                Assert.That(controlPoints.DifficultyPointAt(3000).GenerateTicks, Is.True);
            }
        }

        [Test]
        public void TestSamplePointLeniency()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("sample-point-leniency.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var hitObject = decoder.Decode(stream).HitObjects.Single();
                Assert.That(hitObject.Samples.Select(s => s.Volume), Has.All.EqualTo(70));
            }
        }

        [Test]
        public void TestNewComboAfterBreak()
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("break-between-objects.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var beatmap = decoder.Decode(stream);
                Assert.That(((IHasCombo)beatmap.HitObjects[0]).NewCombo, Is.True);
                Assert.That(((IHasCombo)beatmap.HitObjects[1]).NewCombo, Is.True);
                Assert.That(((IHasCombo)beatmap.HitObjects[2]).NewCombo, Is.False);
            }
        }

        /// <summary>
        /// Test cases that involve a spinner between two hitobjects.
        /// </summary>
        [Test]
        public void TestSpinnerNewComboBetweenObjects([Values("osu", "catch")] string rulesetName)
        {
            var decoder = new LegacyBeatmapDecoder { ApplyOffsets = false };

            using (var resStream = TestResources.OpenResource("spinner-between-objects.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                Ruleset ruleset;

                switch (rulesetName)
                {
                    case "osu":
                        ruleset = new OsuRuleset();
                        break;

                    case "catch":
                        ruleset = new CatchRuleset();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(rulesetName), rulesetName, null);
                }

                var working = new TestWorkingBeatmap(decoder.Decode(stream));
                var playable = working.GetPlayableBeatmap(ruleset.RulesetInfo, Array.Empty<Mod>());

                // There's no good way to figure out these values other than to compare (in code) with osu!stable...

                Assert.That(((IHasComboInformation)playable.HitObjects[0]).ComboIndexWithOffsets, Is.EqualTo(1));
                Assert.That(((IHasComboInformation)playable.HitObjects[2]).ComboIndexWithOffsets, Is.EqualTo(2));
                Assert.That(((IHasComboInformation)playable.HitObjects[3]).ComboIndexWithOffsets, Is.EqualTo(2));
                Assert.That(((IHasComboInformation)playable.HitObjects[5]).ComboIndexWithOffsets, Is.EqualTo(3));
                Assert.That(((IHasComboInformation)playable.HitObjects[6]).ComboIndexWithOffsets, Is.EqualTo(3));
                Assert.That(((IHasComboInformation)playable.HitObjects[8]).ComboIndexWithOffsets, Is.EqualTo(4));
                Assert.That(((IHasComboInformation)playable.HitObjects[9]).ComboIndexWithOffsets, Is.EqualTo(4));
                Assert.That(((IHasComboInformation)playable.HitObjects[11]).ComboIndexWithOffsets, Is.EqualTo(5));
                Assert.That(((IHasComboInformation)playable.HitObjects[12]).ComboIndexWithOffsets, Is.EqualTo(6));
                Assert.That(((IHasComboInformation)playable.HitObjects[14]).ComboIndexWithOffsets, Is.EqualTo(7));
                Assert.That(((IHasComboInformation)playable.HitObjects[15]).ComboIndexWithOffsets, Is.EqualTo(8));
                Assert.That(((IHasComboInformation)playable.HitObjects[17]).ComboIndexWithOffsets, Is.EqualTo(9));
            }
        }
    }
}
