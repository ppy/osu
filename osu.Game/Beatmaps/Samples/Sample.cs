// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Samples
{
    /// <summary>
    /// A <see cref="Sample"/> defines a type of sound that is to be played.
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// The type of sound to be played.
        /// </summary>
        public SampleType Type;

        /// <summary>
        /// The volume to be played at.
        /// </summary>
        public int? Volume;
    }
}
