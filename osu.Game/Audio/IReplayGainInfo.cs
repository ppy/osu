using System;
using osu.Game.Database;

namespace osu.Game.Audio
{
    public interface IReplayGainInfo : IEquatable<IReplayGainInfo>
    {
        public float PeakAmplitude { get;  }
        public float TrackGain { get;  }
    }
}
