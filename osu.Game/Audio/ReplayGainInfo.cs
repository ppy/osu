using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Audio
{
    public class ReplayGainInfo :  IReplayGainInfo, IEquatable<ReplayGainInfo>, IHasPrimaryKey, ISoftDelete
    {
        public int ID { get; set; }

        /*[Column("Beatmap")]
        public BeatmapInfo BeatmapInfo { get; set; }*/

        public float TrackGain { get; set; }
        public float PeakAmplitude { get; set; }
        public float Version { get; set; }
        public bool DeletePending { get; set; }
        public ReplayGainInfo()
        { }
        public bool Equals(ReplayGainInfo other)
        {
            if (TrackGain == other.TrackGain && PeakAmplitude == other.PeakAmplitude)
                return true;
            else
                return false;
        }

        public bool Equals(IReplayGainInfo other) => other is ReplayGainInfo b && Equals(b);
    }
}
