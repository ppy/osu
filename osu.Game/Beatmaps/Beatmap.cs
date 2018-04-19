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
    public interface IBeatmap : IJsonSerializable
    {
        /// <summary>
        /// This beatmap's info.
        /// </summary>
        BeatmapInfo BeatmapInfo { get; }

        /// <summary>
        /// This beatmap's metadata.
        /// </summary>
        BeatmapMetadata Metadata { get; }

        /// <summary>
        /// The control points in this beatmap.
        /// </summary>
        ControlPointInfo ControlPointInfo { get; }

        /// <summary>
        /// The breaks in this beatmap.
        /// </summary>
        List<BreakPeriod> Breaks { get; }

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
        double TotalBreakTime { get; }

        /// <summary>
        /// The hitobjects contained by this beatmap.
        /// </summary>
        IEnumerable<HitObject> HitObjects { get; }

        /// <summary>
        /// Creates a shallow-clone of this beatmap and returns it.
        /// </summary>
        /// <returns>The shallow-cloned beatmap.</returns>
        IBeatmap Clone();
    }

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

        public Beatmap<T> Clone() => new Beatmap<T>
        {
            BeatmapInfo = BeatmapInfo.DeepClone(),
            ControlPointInfo = ControlPointInfo,
            Breaks = Breaks,
            HitObjects = HitObjects
        };

        IBeatmap IBeatmap.Clone() => Clone();
    }

    public class Beatmap : Beatmap<HitObject>
    {
        public new Beatmap Clone() => (Beatmap)base.Clone();
    }
}
