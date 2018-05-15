// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A Beatmap containing converted HitObjects.
    /// </summary>
    public class Beatmap<T> : IBeatmap
        where T : HitObject
    {
        public BeatmapInfo BeatmapInfo { get; set; } = new BeatmapInfo
        {
            Metadata = new BeatmapMetadata
            {
                Artist = @"Unknown",
                Title = @"Unknown",
                AuthorString = @"Unknown Creator",
            },
            Version = @"Normal",
            BaseDifficulty = new BeatmapDifficulty()
        };

        [JsonIgnore]
        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;

        public ControlPointInfo ControlPointInfo { get; set; } = new ControlPointInfo();

        public List<BreakPeriod> Breaks { get; set; } = new List<BreakPeriod>();

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
        [JsonIgnore]
        public double TotalBreakTime => Breaks.Sum(b => b.Duration);

        /// <summary>
        /// The HitObjects this Beatmap contains.
        /// </summary>
        [JsonConverter(typeof(TypedListConverter<HitObject>))]
        public List<T> HitObjects = new List<T>();

        IEnumerable<HitObject> IBeatmap.HitObjects => HitObjects;

        public virtual IEnumerable<BeatmapStatistic> GetStatistics() => Enumerable.Empty<BeatmapStatistic>();

        IBeatmap IBeatmap.Clone() => Clone();

        public Beatmap<T> Clone()
        {
            var newInstance = (Beatmap<T>)MemberwiseClone();
            newInstance.BeatmapInfo = BeatmapInfo.DeepClone();

            return newInstance;
        }
    }

    public class Beatmap : Beatmap<HitObject>
    {
        public Beatmap Clone() => (Beatmap)base.Clone();
    }
}
