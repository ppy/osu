// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass.Loud;
using ManagedBass;

namespace osu.Game.Audio.Effects
{
    public class AudioNormalization
    {
        public AudioNormalization(string file)
        {
            var idk = Bass.CreateStream(file, 0, 0, BassFlags.Decode | BassFlags.Float);

            BassLoud.BASS_Loudness_Start(idk, BassFlags.BassLoudnessIntegrated | BassFlags.BassLoudnessAutofree, 0);

            IntPtr fart = new IntPtr();
            Bass.ChannelGetData(idk, fart, 0);

            float level = 0;
            BassLoud.BASS_Loudness_GetLevel(idk, BassFlags.BassLoudnessIntegrated, level);

            Bass.StreamFree(idk);

            Console.WriteLine("level: " + level);
        }
    }
}
