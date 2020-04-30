// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Audio;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class SampleControlPoint : ControlPoint
    {
        public const string DEFAULT_BANK = "normal";

        /// <summary>
        /// The default sample bank at this control point.
        /// </summary>
        public readonly Bindable<string> SampleBankBindable = new Bindable<string>(DEFAULT_BANK) { Default = DEFAULT_BANK };

        /// <summary>
        /// The speed multiplier at this control point.
        /// </summary>
        public string SampleBank
        {
            get => SampleBankBindable.Value;
            set => SampleBankBindable.Value = value;
        }

        /// <summary>
        /// The default sample bank at this control point.
        /// </summary>
        public readonly BindableInt SampleVolumeBindable = new BindableInt(100)
        {
            MinValue = 0,
            MaxValue = 100,
            Default = 100
        };

        /// <summary>
        /// The default sample volume at this control point.
        /// </summary>
        public int SampleVolume
        {
            get => SampleVolumeBindable.Value;
            set => SampleVolumeBindable.Value = value;
        }

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

        public override bool IsRedundant(ControlPoint existing)
            => existing is SampleControlPoint existingSample
               && SampleBank == existingSample.SampleBank
               && SampleVolume == existingSample.SampleVolume;
    }
}
