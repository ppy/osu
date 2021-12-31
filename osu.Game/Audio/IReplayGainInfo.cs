using System;
using osu.Game.Database;

namespace osu.Game.Audio
{
    public interface IReplayGainInfo : IEquatable<IReplayGainInfo>
    {
        public float PeakAmplitude { get; set; }
        public float TrackGain { get; set; }
        public float Version { get; set; }
    }
}
