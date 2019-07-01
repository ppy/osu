// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Audio;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class SampleControlPoint : ControlPoint, IEquatable<SampleControlPoint>
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
        /// <returns>A populated <see cref="HitSampleInfo"/>.</returns>
        public HitSampleInfo GetSampleInfo(string sampleName = HitSampleInfo.HIT_NORMAL) => new HitSampleInfo
        {
            Bank = SampleBank,
            Name = sampleName,
            Volume = SampleVolume,
        };

        /// <summary>
        /// Applies <see cref="SampleBank"/> and <see cref="SampleVolume"/> to a <see cref="HitSampleInfo"/> if necessary, returning the modified <see cref="HitSampleInfo"/>.
        /// </summary>
        /// <param name="hitSampleInfo">The <see cref="HitSampleInfo"/>. This will not be modified.</param>
        /// <returns>The modified <see cref="HitSampleInfo"/>. This does not share a reference with <paramref name="hitSampleInfo"/>.</returns>
        public virtual HitSampleInfo ApplyTo(HitSampleInfo hitSampleInfo)
        {
            var newSampleInfo = hitSampleInfo.Clone();
            newSampleInfo.Bank = hitSampleInfo.Bank ?? SampleBank;
            newSampleInfo.Volume = hitSampleInfo.Volume > 0 ? hitSampleInfo.Volume : SampleVolume;
            return newSampleInfo;
        }

        public bool Equals(SampleControlPoint other)
            => base.Equals(other)
               && string.Equals(SampleBank, other?.SampleBank) && SampleVolume == other?.SampleVolume;
    }
}
