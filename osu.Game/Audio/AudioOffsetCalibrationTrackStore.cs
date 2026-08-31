// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.IO.Stores;

namespace osu.Game.Audio
{
    /// <summary>
    /// Sample-accurate, uncompressed 60 and 120 BPM reference tracks. Clicks are part of the audio stream,
    /// rather than samples scheduled on the update thread.
    /// </summary>
    public class AudioOffsetCalibrationTrackStore : IResourceStore<byte[]>
    {
        public const string TRACK_NAME = "offset-calibration.wav";
        public const string SLOW_TRACK_NAME = "offset-calibration-slow.wav";
        public const int SAMPLE_RATE = 48000;
        public const int BEAT_LENGTH = 500;
        public const int SLOW_BEAT_LENGTH = 1000;
        public const int FIRST_BEAT = 250;
        public const int BEAT_COUNT = 16;

        private readonly byte[] data = createTrack(BEAT_LENGTH);
        private readonly byte[] slowData = createTrack(SLOW_BEAT_LENGTH);

        private static byte[] createTrack(int beatLength)
        {
            int samplesPerBeat = SAMPLE_RATE * beatLength / 1000;
            int sampleCount = samplesPerBeat * BEAT_COUNT;
            const int click_samples = SAMPLE_RATE / 100;

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.ASCII, true);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + sampleCount * sizeof(short));
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM.
            writer.Write((short)1); // Mono.
            writer.Write(SAMPLE_RATE);
            writer.Write(SAMPLE_RATE * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(sampleCount * sizeof(short));

            for (int i = 0; i < sampleCount; i++)
            {
                int relativeSample = i - SAMPLE_RATE * FIRST_BEAT / 1000;
                int clickSample = relativeSample % samplesPerBeat;
                double amplitude = 0;

                if (relativeSample >= 0 && clickSample < click_samples)
                {
                    // Accentuate the first beat of each bar. The envelope ends at zero to avoid a discontinuity.
                    double frequency = relativeSample / samplesPerBeat % 4 == 0 ? 1500 : 1000;
                    double envelope = 1 - (double)clickSample / (click_samples - 1);
                    amplitude = 0.5 * envelope * envelope * Math.Sin(2 * Math.PI * frequency * clickSample / SAMPLE_RATE);
                }

                writer.Write((short)(amplitude * short.MaxValue));
            }

            return stream.ToArray();
        }

        public byte[] Get(string name) => name switch
        {
            TRACK_NAME => data,
            SLOW_TRACK_NAME => slowData,
            _ => null!,
        };

        public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Get(name));
        }

        public Stream? GetStream(string name) => Get(name) is byte[] bytes ? new MemoryStream(bytes, false) : null;

        public IEnumerable<string> GetAvailableResources() => new[] { TRACK_NAME, SLOW_TRACK_NAME };

        public void Dispose()
        {
        }
    }
}
