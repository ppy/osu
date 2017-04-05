// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Beatmaps.Samples
{
    /// <summary>
    /// Wraps a list of <see cref="Sample"/> to change which bank of files are used for each <see cref="Sample"/>.
    /// </summary>
    public class SampleBank
    {
        /// <summary>
        /// The list of samples that are to be played to be played from this bank.
        /// </summary>
        public List<Sample> Sets;

        /// <summary>
        /// In conversion from osu-stable, this is equivalent to SampleSet (_not_ CustomSampleSet).
        /// i.e. None/Normal/Soft/Drum
        /// </summary>
        public string Name;

        /// <summary>
        /// Default sample volume.
        /// </summary>
        public int Volume;
    }
}
