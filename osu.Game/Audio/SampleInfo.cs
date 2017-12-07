// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Audio
{
    public class SampleInfo
    {
        public const string HIT_WHISTLE = @"hitwhistle";
        public const string HIT_FINISH = @"hitfinish";
        public const string HIT_NORMAL = @"hitnormal";
        public const string HIT_CLAP = @"hitclap";

        public static SampleInfo FromSoundPoint(SoundControlPoint soundPoint, string sampleName = SampleInfo.HIT_NORMAL)
        {
            return new SampleInfo()
            {
                Bank = soundPoint.SampleBank,
                Name = sampleName,
                Volume = soundPoint.SampleVolume,
            };
        }

        public SampleChannel GetChannel(SampleManager manager)
        {
            var channel = manager.Get($"{Bank}-{Name}");

            channel.AddAdjustment(AdjustableProperty.Volume, new BindableDouble(Volume / 100.0));
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
