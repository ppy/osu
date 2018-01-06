// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Sample;

namespace osu.Game.Audio
{
    [Serializable]
    public class SampleInfo
    {
        public const string HIT_WHISTLE = @"hitwhistle";
        public const string HIT_FINISH = @"hitfinish";
        public const string HIT_NORMAL = @"hitnormal";
        public const string HIT_CLAP = @"hitclap";

        public SampleChannel GetChannel(SampleManager manager, string resourceNamespace = null)
        {
            SampleChannel channel = null;

            if (resourceNamespace != null)
                channel = manager.Get($"Gameplay/{resourceNamespace}/{Bank}-{Name}");

            // try without namespace as a fallback.
            if (channel == null)
                channel = manager.Get($"Gameplay/{Bank}-{Name}");

            if (channel != null)
                channel.Volume.Value = Volume / 100.0;

            return channel;
        }

        /// <summary>
        /// The bank to load the sample from.
        /// </summary>
        public string Bank;

        /// <summary>
        /// The name of the sample to load.
        /// </summary>
        public string Name;

        /// <summary>
        /// The sample volume.
        /// </summary>
        public int Volume;
    }
}
