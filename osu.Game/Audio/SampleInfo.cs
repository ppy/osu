// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.Audio
{
    [Serializable]
    public class SampleInfo
    {
        public const string HIT_WHISTLE = @"hitwhistle";
        public const string HIT_FINISH = @"hitfinish";
        public const string HIT_NORMAL = @"hitnormal";
        public const string HIT_CLAP = @"hitclap";

        /// <summary>
        /// An optional ruleset namespace.
        /// </summary>
        public string Namespace;

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

        /// <summary>
        /// Retrieve all possible filenames that can be used as a source, returned in order of preference (highest first).
        /// </summary>
        public virtual IEnumerable<string> LookupNames
        {
            get
            {
                if (!string.IsNullOrEmpty(Namespace))
                    yield return $"{Namespace}/{Bank}-{Name}";

                yield return $"{Bank}-{Name}"; // Without namespace as a fallback even when we have a namespace
            }
        }

        public SampleInfo Clone() => (SampleInfo)MemberwiseClone();
    }
}
