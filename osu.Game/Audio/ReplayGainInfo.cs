using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using Realms;

namespace osu.Game.Audio
{
    [ExcludeFromDynamicCompile]
    public class ReplayGainInfo : RealmObject, IReplayGainInfo, IEquatable<ReplayGainInfo>, IHasGuidPrimaryKey
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public float TrackGain { get; set; }
        public float PeakAmplitude { get; set; }
        public bool DeletePending { get; set; }

        [UsedImplicitly]
        public ReplayGainInfo()
        { }

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

        public bool Equals(IReplayGainInfo other) => other is ReplayGainInfo b && Equals(b);

        //public ReplayGainInfo Clone() => (ReplayGainInfo)this.Detach().MemberwiseClone();

        public ReplayGainInfo Clone()
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
