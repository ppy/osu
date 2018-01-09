// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Audio;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class SampleControlPoint : ControlPoint
    {
        public const string DEFAULT_BANK = "normal";

        /// <summary>
        /// The default sample bank at this control point.
        /// </summary>
        public string SampleBank = DEFAULT_BANK;

        /// <summary>
        /// The default sample volume at this control point.
        /// </summary>
        public int SampleVolume = 100;

        /// <summary>
        /// Create a SampleInfo based on the sample settings in this control point.
        /// </summary>
        /// <param name="sampleName">The name of the same.</param>
        /// <returns>A populated <see cref="SampleInfo"/>.</returns>
        public SampleInfo GetSampleInfo(string sampleName = SampleInfo.HIT_NORMAL) => new SampleInfo
        {
            Bank = SampleBank,
            Name = sampleName,
            Volume = SampleVolume,
        };
    }
}
