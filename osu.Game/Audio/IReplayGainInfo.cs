using System;
using osu.Game.Database;

namespace osu.Game.Audio
{
    public interface IReplayGainInfo : IEquatable<IReplayGainInfo>, ISoftDelete
    {
        public float PeakAmplitude { get;  }
        public float TrackGain { get;  }
    }
}
