using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Audio
{
    [ExcludeFromDynamicCompile]
    public class ReplayGainInfo : RealmObject, IReplayGainInfo, IHasGuidPrimaryKey, IEquatable<ReplayGainInfo>, IDeepCloneable<ReplayGainInfo>
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public float TrackGain { get; set; }
        public float PeakAmplitude { get; set; }

        [UsedImplicitly]
        public ReplayGainInfo()
        {
            ID = Guid.NewGuid();
            TrackGain = 0;
            PeakAmplitude = 0;
        }

        public ReplayGainInfo(float peakAmp, float gain)
        {
            ID = Guid.NewGuid();
            TrackGain = gain;
            PeakAmplitude = peakAmp;
        }

        public bool Equals(ReplayGainInfo other)
        {
            if (TrackGain == other.TrackGain && PeakAmplitude == other.PeakAmplitude)
                return true;
            else
                return false;
        }

        public bool isDefault()
        {
            if(TrackGain == 0 && PeakAmplitude == 0)
                return true;
            return false;
        }

        public bool Equals(IReplayGainInfo other) => other is ReplayGainInfo b && Equals(b);

        public ReplayGainInfo DeepClone()
        {
            return new ReplayGainInfo()
            {
                ID  = ID,
                TrackGain = TrackGain,
                PeakAmplitude = PeakAmplitude,
            };
        }
    }
}
