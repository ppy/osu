// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass;
using ManagedBass.Loud;

namespace osu.Game.Audio
{
    public class BassAudioNormalization
    {
        public float IntegratedLoudness { get; }

        public BassAudioNormalization(string filePath)
        {
            int decodeStream = Bass.CreateStream(filePath, 0, 0, BassFlags.Decode | BassFlags.Float);

            if (decodeStream == 0)
            {
                throw new InvalidOperationException("Failed to create stream!\nError Code: " + Bass.LastError);
            }

            int loudness = BassLoud.BASS_Loudness_Start(decodeStream, BassFlags.BassLoudnessIntegrated | BassFlags.BassLoudnessAutofree, 0);

            if (loudness == 0)
            {
                throw new InvalidOperationException("Failed to start loudness measurement!\nError Code: " + Bass.LastError);
            }

            byte[] buffer = new byte[10000];

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

            Bass.SampleFree(decodeStream);
        }
    }
}
