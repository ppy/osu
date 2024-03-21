// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using ManagedBass.Loud;
using ManagedBass;

namespace osu.Game.Audio.Effects
{
    public class AudioNormalization
    {
        public AudioNormalization(string file)
        {
            Bass.Free();
            Bass.Init();
            Console.WriteLine("bassloud version: " + BassLoud.Version);
            var idk = Bass.CreateStream(file, 0, 0, BassFlags.Decode | BassFlags.Float);
            Console.WriteLine("idk: " + idk);

            if (idk == 0)
            {
                Console.WriteLine("cant decode stream");
                Console.WriteLine(Bass.LastError);
                return;
            }

            var loudness = BassLoud.BASS_Loudness_Start(idk, BassFlags.BassLoudnessIntegrated, 0);
            Console.WriteLine(Bass.LastError);

            Console.WriteLine("loudness: " + loudness);

            if (loudness == 0)
            {
                Console.WriteLine("cant start loudness measurement");
                Console.WriteLine(Bass.LastError);
                return;
            }

            int data;
            while (true)
            {
                IntPtr buffer = new IntPtr();
                data = Bass.ChannelGetData(idk, buffer,Marshal.SizeOf(buffer));

                if (data < 0) break;
            }

            Bass.StreamFree(idk);

            float level = 0;
            var gotlevel = BassLoud.BASS_Loudness_GetLevel(loudness, BassFlags.BassLoudnessIntegrated, level);
            Console.WriteLine("level: " + level);
            Console.WriteLine("gotlevel: " + gotlevel);

            if (gotlevel == false)
            {
                Console.WriteLine("failed to get level");
                Console.WriteLine(Bass.LastError);
            }

        }
    }
}
