// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Game.Audio;

namespace osu.Game.Tests.NonVisual.Audio
{
    [TestFixture]
    public class AudioOffsetCalibrationTrackStoreTest
    {
        [Test]
        public void TestPcmFormatAndBeatSpacing([Values] bool slow)
        {
            using var store = new AudioOffsetCalibrationTrackStore();
            using var stream = store.GetStream(slow ? AudioOffsetCalibrationTrackStore.SLOW_TRACK_NAME : AudioOffsetCalibrationTrackStore.TRACK_NAME)!;
            using var reader = new BinaryReader(stream);
            int seconds = slow ? 16 : 8;
            int samplesPerBeat = slow ? 48000 : 24000;

            Assert.That(Encoding.ASCII.GetString(reader.ReadBytes(4)), Is.EqualTo("RIFF"));
            Assert.That(reader.ReadInt32(), Is.EqualTo(stream.Length - 8));
            Assert.That(Encoding.ASCII.GetString(reader.ReadBytes(8)), Is.EqualTo("WAVEfmt "));
            Assert.That(reader.ReadInt32(), Is.EqualTo(16));
            Assert.That(reader.ReadInt16(), Is.EqualTo(1));
            Assert.That(reader.ReadInt16(), Is.EqualTo(1));
            Assert.That(reader.ReadInt32(), Is.EqualTo(48000));
            Assert.That(reader.ReadInt32(), Is.EqualTo(96000));
            Assert.That(reader.ReadInt16(), Is.EqualTo(2));
            Assert.That(reader.ReadInt16(), Is.EqualTo(16));
            Assert.That(Encoding.ASCII.GetString(reader.ReadBytes(4)), Is.EqualTo("data"));
            Assert.That(reader.ReadInt32(), Is.EqualTo(48000 * seconds * 2));

            short[] samples = new short[48000 * seconds];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = reader.ReadInt16();

            for (int beat = 0; beat < 16; beat++)
            {
                int onset = 12000 + beat * samplesPerBeat;
                Assert.That(samples[onset], Is.Zero);
                Assert.That(samples[onset + 1], Is.GreaterThan(0), $"Missing click at beat {beat}");
                Assert.That(samples[onset - 1], Is.Zero);
                Assert.That(samples[onset + 479], Is.Zero);
                Assert.That(Array.Exists(samples[onset..(onset + 480)], s => Math.Abs((int)s) > 1000), Is.True);
            }

            // No extra clicks in the gaps or at the loop seam.
            for (int i = 0; i < samples.Length; i++)
            {
                if (i < 12000 || (i - 12000) % samplesPerBeat >= 480)
                    Assert.That(samples[i], Is.Zero);
            }
        }

        [Test]
        public void TestStreamsHaveIndependentPositions()
        {
            using var store = new AudioOffsetCalibrationTrackStore();
            using var first = store.GetStream(AudioOffsetCalibrationTrackStore.TRACK_NAME)!;
            using var second = store.GetStream(AudioOffsetCalibrationTrackStore.TRACK_NAME)!;
            first.ReadByte();
            Assert.That(second.Position, Is.Zero);
            Assert.That(store.GetStream("missing.wav"), Is.Null);
        }
    }
}
