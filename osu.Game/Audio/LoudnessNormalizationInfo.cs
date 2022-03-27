// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Audio
{
    [ExcludeFromDynamicCompile]
    public class LoudnessNormalizationInfo : RealmObject, ILoudnessNormalizationInfo, IHasGuidPrimaryKey, IEquatable<LoudnessNormalizationInfo>, IDeepCloneable<LoudnessNormalizationInfo>
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public float TrackGain { get; set; }
        public float PeakAmplitude { get; set; }

        [UsedImplicitly]
        public LoudnessNormalizationInfo()
        {
        }

        public LoudnessNormalizationInfo(float peakAmp, float gain)
        {
            ID = Guid.NewGuid();
            TrackGain = gain;
            PeakAmplitude = peakAmp;
        }

        public bool Equals(LoudnessNormalizationInfo other)
        {
            if (TrackGain == other?.TrackGain && PeakAmplitude == other.PeakAmplitude)
                return true;
            else
                return false;
        }

        public bool IsDefault()
        {
            if (TrackGain == 0 && PeakAmplitude == 0)
                return true;
            else
                return false;
        }

        public bool Equals(ILoudnessNormalizationInfo other) => other is LoudnessNormalizationInfo b && Equals(b);

        public LoudnessNormalizationInfo DeepClone()
        {
            return new LoudnessNormalizationInfo
            {
                ID = ID,
                TrackGain = TrackGain,
                PeakAmplitude = PeakAmplitude,
            };
        }
    }
}
