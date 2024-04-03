// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass;
using ManagedBass.Loud;

namespace osu.Game.Audio
{
    public class BassAudioNormalization
    {
        private const int target_level = -23;

        public float VolumeOffset { get; set; }

        public float IntegratedLoudness { get; set; }

        public BassAudioNormalization(string? filePath)
        {
            if (filePath != null)
            {
                calculateLoudness(filePath);
            }
            else
            {
                VolumeOffset = 0;
                IntegratedLoudness = 0;
            }
        }

        private void calculateLoudness(string? filePath)
        {
            int decodeStream = Bass.CreateStream(filePath, 0, 0, BassFlags.Decode | BassFlags.Float);

            if (decodeStream == 0)
            {
                VolumeOffset = 0;
                IntegratedLoudness = 0;
                return;
            }

            int loudness = BassLoud.BASS_Loudness_Start(decodeStream, BassFlags.BassLoudnessIntegrated | BassFlags.BassLoudnessAutofree, 0);

            if (loudness == 0)
            {
                throw new InvalidOperationException("Failed to start loudness measurement!\nError Code: " + Bass.LastError);
            }

            byte[] buffer = new byte[65536];

            while (Bass.ChannelIsActive(decodeStream) == PlaybackState.Playing)
            {
                Bass.ChannelGetData(decodeStream, buffer, buffer.Length);
            }

            float integratedLoudness = IntegratedLoudness;
            bool gotLevel = BassLoud.BASS_Loudness_GetLevel(loudness, BassFlags.BassLoudnessIntegrated, ref integratedLoudness);

            if (!gotLevel)
            {
                throw new InvalidOperationException("Failed to get level!\nError Code: " + Bass.LastError);
            }

            IntegratedLoudness = integratedLoudness;
            VolumeOffset = (float)Math.Pow(10, (target_level - integratedLoudness) / 20);
        }
    }
}
