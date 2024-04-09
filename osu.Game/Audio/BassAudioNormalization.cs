// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Loud;
using osu.Framework.Logging;

namespace osu.Game.Audio
{
    public class BassAudioNormalization
    {
        /// <summary>
        /// The integrated loudness of the audio
        /// Applicable range = -70 to -1
        /// Returns 1 if the loudness could not be measured possibly due to an error
        /// </summary>
        public float IntegratedLoudness { get; }

        public BassAudioNormalization(string filePath)
        {
            int decodeStream = Bass.CreateStream(filePath, 0, 0, BassFlags.Decode | BassFlags.Float);

            if (decodeStream == 0)
            {
                Logger.Log("Failed to create stream for loudness measurement!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = 1;
            }

            int loudness = BassLoud.BASS_Loudness_Start(decodeStream, BassFlags.BassLoudnessIntegrated | BassFlags.BassLoudnessAutofree, 0);

            if (loudness == 0)
            {
                Logger.Log("Failed to start loudness measurement!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = 1;
            }

            byte[] buffer = new byte[10000];

            while (Bass.ChannelGetData(decodeStream, buffer, buffer.Length) >= 0)
            {
            }

            float integratedLoudness = IntegratedLoudness;
            bool gotLevel = BassLoud.BASS_Loudness_GetLevel(loudness, BassFlags.BassLoudnessIntegrated, ref integratedLoudness);

            IntegratedLoudness = integratedLoudness;

            if (!gotLevel)
            {
                Logger.Log("Failed to get loudness level!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = 1;
            }

            var freedStream = Bass.StreamFree(decodeStream);
            if (!freedStream)
                Logger.Log("Failed to free stream!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
        }
    }
}
