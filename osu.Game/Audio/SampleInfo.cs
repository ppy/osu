// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Audio
{
    [Serializable]
    public class SampleInfo
    {
        public const string HIT_WHISTLE = @"hitwhistle";
        public const string HIT_FINISH = @"hitfinish";
        public const string HIT_NORMAL = @"hitnormal";
        public const string HIT_CLAP = @"hitclap";

        [JsonIgnore]
        public SoundControlPoint ControlPoint;

        private string bank;
        /// <summary>
        /// The bank to load the sample from.
        /// </summary>
        public string Bank
        {
            get { return string.IsNullOrEmpty(bank) ? (ControlPoint?.SampleBank ?? "normal") : bank; }
            set { bank = value; }
        }

        public bool ShouldSerializeBank() => Bank == ControlPoint.SampleBank;

        /// <summary>
        /// The name of the sample to load.
        /// </summary>
        public string Name { get; set; }

        private int volume;
        /// <summary>
        /// The sample volume.
        /// </summary>
        public int Volume
        {
            get { return volume == 0 ? (ControlPoint?.SampleVolume ?? 0) : volume; }
            set { volume = value; }
        }

        public bool ShouldSerializeVolume() => Volume == ControlPoint.SampleVolume;
    }
}
