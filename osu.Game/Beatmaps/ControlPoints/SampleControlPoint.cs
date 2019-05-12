// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        /// <summary>
        /// Applies <see cref="SampleBank"/> and <see cref="SampleVolume"/> to a <see cref="SampleInfo"/> if necessary, returning the modified <see cref="SampleInfo"/>.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="SampleInfo"/>. This will not be modified.</param>
        /// <returns>The modified <see cref="SampleInfo"/>. This does not share a reference with <paramref name="sampleInfo"/>.</returns>
        public virtual SampleInfo ApplyTo(SampleInfo sampleInfo)
        {
            var newSampleInfo = sampleInfo.Clone();
            newSampleInfo.Bank = sampleInfo.Bank ?? SampleBank;
            newSampleInfo.Volume = sampleInfo.Volume > 0 ? sampleInfo.Volume : SampleVolume;
            return newSampleInfo;
        }

        public override bool EquivalentTo(ControlPoint other)
            => base.EquivalentTo(other)
               && other is SampleControlPoint sample
               && SampleBank.Equals(sample.SampleBank)
               && SampleVolume.Equals(sample.SampleVolume);
    }
}
